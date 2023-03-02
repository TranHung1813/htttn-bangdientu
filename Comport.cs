using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Display
{
    public class ComPort
    {
        private SerialPort Uart2Com;

        private int _MaxSizeRingBuff = 0;
        private int _MaxSizePacket = 0;
        private int _Baudrate = 0;
        private int _Databit = 0;
        private StopBits _StopBit = StopBits.One;
        private Parity _Par = Parity.None;

        private bool _EncodeEnable = false;

        public static int E_OK = 0;
        public static int E_NOT_OK = 1;

        private byte[] _RxRingBuff;
        private int _RxHead = 0;
        private int _RxTail = 0;
        private bool _RxRingBuff_Ready = false;

        Thread SendManager_trd;
        Thread RecvManager_trd;
        Thread FindComPort_trd;

        private byte[] _FindComDataRx;
        private int _FindComDataRx_Length = 0;

        public static int MAX_BUFFER_SEND = 32;
        private const int TIME_DETECT_PACKET = 100;
        public static int MAX_TRY_SEND_NUMBER = 5;

        static byte[,] tbBufferSend;
        static int[] LengthSend = new int[MAX_BUFFER_SEND];

        private static int CountBuffer = 0;
        private static int[] CountNumSend = new int[MAX_BUFFER_SEND];
        private static bool[] CyclicStatus = new bool[MAX_BUFFER_SEND];
        private static int[] CyclicPeriod = new int[MAX_BUFFER_SEND];

        private const int CONNECT_COM_STATE = 0x00;
        private const int CONNECT_SENDPING_STATE = 0x01;
        private const int CONNECT_WAITPONG_STATE = 0x02;

        private byte[] _PingPacket = { 0 };
        private byte[] _PongPacket;
        private int _PingPacket_Length = 0;
        private int _PongPacket_Length = 0;
        private int _CyclicPeriod = 0;
        private bool _AutoConnect = false;

        private int _ChannelSendPing = 0;
        public bool AutoConnect
        {
            get => _AutoConnect;
            set => _AutoConnect = value;
        }
        private const int MINTIME_SENDPING = 10;

        public int SetupComPort(String PortName, int Baudrate, int Databit, StopBits StopBit, Parity Par)
        {
            int ret = E_NOT_OK;

            try
            {
                if (Uart2Com.IsOpen == false)
                {
                    Uart2Com.PortName = PortName;
                    Uart2Com.BaudRate = Baudrate;
                    Uart2Com.DataBits = Databit;
                    Uart2Com.StopBits = StopBit;
                    Uart2Com.Parity = Par;


                    _Baudrate = Baudrate;
                    _Databit = Databit;
                    _StopBit = StopBit;
                    _Par = Par;

                    _RxHead = 0;
                    _RxTail = 0;

                    Uart2Com.DataReceived += new SerialDataReceivedEventHandler(this.Serial_ReceiverHandler);
                    Uart2Com.Open();

                    try
                    {
                        SendManager_trd.Abort();
                        RecvManager_trd.Abort();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("SetupComPort_Error: " + ex.Message);
                    }

                    SendManager_trd = new Thread(new ThreadStart(this.SendManager_Thread));
                    SendManager_trd.IsBackground = true;
                    SendManager_trd.Start();

                    RecvManager_trd = new Thread(new ThreadStart(this.ReceiverManager_Thread));
                    RecvManager_trd.IsBackground = true;
                    RecvManager_trd.Start();

                    ret = E_OK;
                }
            }
            catch (Exception exx)
            {
                Console.WriteLine("SetupComPort_Error2: " + exx.Message);

                if (Uart2Com.IsOpen == true)
                {
                    Uart2Com.Close();
                }
            }

            return ret;
        }

        public void DeleteRecBuff()
        {
            _RxHead = _RxTail = 0;
        }
        public int Close()
        {
            int ret = E_NOT_OK;
            try
            {
                if (Uart2Com.IsOpen == true)
                {
                    Uart2Com.Close();
                    if (Uart2Com.IsOpen == false)
                    {
                        try
                        {
                            if (SendManager_trd != null)
                            {
                                SendManager_trd.Abort();
                            }

                            if (RecvManager_trd != null)
                            {
                                RecvManager_trd.Abort();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error_Close: " + ex.Message);
                        }
                        ret = E_OK;
                        //OnNotifyCloseConnect();
                    }

                }
            }
            catch (Exception exx)
            {
                Console.WriteLine("Error_Close2: " + exx.Message);
            }
            return ret;
        }

        public int Send(byte[] data, int length)
        {
            int ret = E_NOT_OK;

            if (length > _MaxSizePacket || Uart2Com.IsOpen == false)
            {
                return ret;
            }

            byte[] dataEnc;
            int lengthEnc;

            if (_EncodeEnable == true)
            {
                lengthEnc = length + 1;
                dataEnc = Encode(data, length);
            }
            else
            {
                dataEnc = data;
                lengthEnc = length;
            }

            Uart2Com.WriteTimeout = 100;
            try
            {
                Uart2Com.Write(dataEnc, 0, lengthEnc);
                //OnNotifySendRawPacket(ComPort.E_OK, dataEnc, lengthEnc);
                ret = E_OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send_Error: " + ex.Message);
                //OnNotifySendRawPacket(ComPort.E_NOT_OK, dataEnc, lengthEnc);
                ret = E_NOT_OK;
            }
            return ret;
        }

        private int RxBufferFindByte(byte byteTarget, ref int local)
        {
            int ret = E_NOT_OK;
            int index = 0;
            int numByte = 0;

            if (_RxHead == _RxTail)
            {
                return ret;
            }

            numByte = _RxTail;

            while (numByte != _RxHead)
            {
                if (++numByte == _MaxSizeRingBuff)
                {
                    numByte = 0;
                }
                index++;
                if (_RxRingBuff[numByte] == byteTarget)
                {
                    local = index;
                    ret = E_OK;
                    break;
                }
            }

            return ret;
        }

        private void DeleteRxData(int dlSize)
        {
            int LengthCur = 0;

            if (_RxHead > _RxTail)
            {
                LengthCur = _RxHead - _RxTail;
            }
            else
            {
                LengthCur = _MaxSizeRingBuff - _RxTail;
                LengthCur += _RxHead;
            }

            if (LengthCur < dlSize)
            {
                _RxTail = _RxHead;
            }
            else
            {
                _RxTail = _RxTail + dlSize;

                if (_RxTail >= _MaxSizeRingBuff)
                {
                    _RxTail = _RxTail - _MaxSizeRingBuff;
                }
            }

        }

        private int RxBufferReadByte(ref byte data)
        {
            int ret = E_NOT_OK;

            if (_RxTail != _RxHead)
            {
                data = _RxRingBuff[_RxTail];
                if (++_RxTail == _MaxSizeRingBuff)
                {
                    _RxTail = 0;
                }
                ret = E_OK;
            }

            return ret;
        }
        public int Recv(ref byte[] data, ref int length)
        {
            int ret = E_NOT_OK;
            int LengthCur = 0;

            if (_RxRingBuff_Ready == true)
            {
                _RxRingBuff_Ready = false;
            }
            else
            {
                return ret;
            }
            if (_RxHead == _RxTail)
            {
                return ret;
            }

            if (_RxHead > _RxTail)
            {
                LengthCur = _RxHead - _RxTail;
            }
            else
            {
                LengthCur = _MaxSizeRingBuff - _RxTail;
                LengthCur += _RxHead;
            }

            if (LengthCur > _MaxSizePacket)
            {
                _RxHead = _RxTail;
                return ret;
            }

            //get buffer data
            data = new byte[LengthCur];
            for (int CountByte = 0; CountByte < LengthCur; CountByte++)
            {
                data[CountByte] = _RxRingBuff[_RxTail];
                _RxTail++;
                if (_RxTail == _MaxSizeRingBuff)
                {
                    _RxTail = 0;
                }
            }

            //OnNotifyRecvRawPacket(data, LengthCur);
            ret = E_OK;

            if (_EncodeEnable == true && ret == E_OK)
            {
                length = LengthCur - 1;
                data = Decode(data, LengthCur);
            }
            else
            {
                length = LengthCur;
            }

            return ret;
        }

        public int DataAvailable()
        {
            int ret = 0;
            if (_RxHead == _RxTail)
            {
                return ret;
            }

            if (_RxHead > _RxTail)
            {
                ret = _RxHead - _RxTail;
            }
            else
            {
                ret = _MaxSizeRingBuff - _RxTail;
                ret += _RxHead;
            }
            return ret;
        }
        public ComPort (int Baudrate, int MaxSizeBuff, bool EncodeEnable)
        {
            _Baudrate = Baudrate;
            _MaxSizePacket = MaxSizeBuff;
            _MaxSizeRingBuff = 8 * MaxSizeBuff;
            _RxRingBuff = new byte[_MaxSizeRingBuff];
            _EncodeEnable = EncodeEnable;

            tbBufferSend = new byte[MAX_BUFFER_SEND, _MaxSizePacket];
            Uart2Com = new SerialPort();

            SendManager_trd = new Thread(new ThreadStart(this.SendManager_Thread));
            SendManager_trd.IsBackground = true;
            SendManager_trd.Start();

            RecvManager_trd = new Thread(new ThreadStart(this.ReceiverManager_Thread));
            RecvManager_trd.IsBackground = true;
            RecvManager_trd.Start();

        }

        private byte[] Encode(byte[] srcData, int length)
        {
            Random rnd = new Random();
            byte key = (byte)rnd.Next(1, 255);
            byte[] desData = new byte[length + 1];

            desData[0] = key;

            for (int CountByte = 0; CountByte < length; CountByte++)
            {
                desData[CountByte + 1] = (byte)(srcData[CountByte] ^ key);
            }

            return desData;
        }

        private byte[] Decode(byte[] srcData, int length)
        {
            byte key = srcData[0];
            byte[] desData;

            if (length > 0)
            {
                desData = new byte[length - 1];

                for (int CountByte = 0; CountByte < length - 1; CountByte++)
                {
                    desData[CountByte] = (byte)(srcData[CountByte + 1] ^ key);
                }
            }
            else
            {
                desData = new byte[1];
            }

            return desData;
        }

        private void Serial_ReceiverHandler(object sender, SerialDataReceivedEventArgs e)
        {
            int DataLength = 0;

            DataLength = Uart2Com.BytesToRead;

            if (DataLength != 0)
            {
                byte[] DataBuff = new byte[5 * _MaxSizePacket + 2];
                if (DataLength >= (5 * _MaxSizePacket + 2))
                {
                    Uart2Com.Read(DataBuff, 0, 5 * _MaxSizePacket + 2);
                    DataLength = 5 * _MaxSizePacket + 2;
                }
                else
                {
                    Uart2Com.Read(DataBuff, 0, DataLength);
                }

                for (int CountByte = 0; CountByte < DataLength; CountByte++)
                {
                    _RxRingBuff[_RxHead] = DataBuff[CountByte];
                    _RxHead++;
                    if (_RxTail != 0)
                    {
                        if (_RxHead == _MaxSizeRingBuff)
                        {
                            _RxHead = 0;
                        }
                        else if (_RxHead == _RxTail)
                        {
                            _RxHead--;
                        }
                    }
                    else
                    {
                        if (_RxHead == _MaxSizeRingBuff)
                        {
                            _RxHead--;
                        }
                    }
                }
                _RxRingBuff_Ready = true;
            }
        }

        public int GetChanelFree()
        {
            int ret = -1;
            for (int CountBuff = 1; CountBuff < MAX_BUFFER_SEND; CountBuff++)
            {
                if (LengthSend[CountBuff] == 0)
                {
                    ret = CountBuff;
                    break;
                }
            }

            return ret;
        }

        public void SetupChannel(int channel, bool CyclicEnable = false, int time_cyclic = 100)
        {
            if (channel < 0 || channel > MAX_BUFFER_SEND)
            {
                return;
            }

            if (CyclicEnable == false)
            {
                CyclicStatus[channel] = false;
                CyclicPeriod[channel] = 1;
            }
            else
            {
                CyclicStatus[channel] = true;
                CyclicPeriod[channel] = time_cyclic;

            }

            LengthSend[channel] = 0;
        }

        public int SendPacket(int channel, byte[] data, int length, bool CyclicEnable = false, int time_cyclic = 100)
        {
            int ret = E_NOT_OK;

            if (channel < 1 || channel > MAX_BUFFER_SEND)
            {
                return ret;
            }

            SetupChannel(channel, CyclicEnable, time_cyclic);
            for (int Countbyte = 0; Countbyte < length; Countbyte++)
            {
                tbBufferSend[channel, Countbyte] = data[Countbyte];
            }
            LengthSend[channel] = length;

            ret = E_OK;
            return ret;
        }
        public int SendPacketPing(byte[] data, int length, bool CyclicEnable = false, int time_cyclic = 100)
        {
            int ret = E_NOT_OK;

            SetupChannel(0, CyclicEnable, time_cyclic);
            for (int Countbyte = 0; Countbyte < length; Countbyte++)
            {
                tbBufferSend[0, Countbyte] = data[Countbyte];
            }
            LengthSend[0] = length;

            ret = E_OK;
            return ret;
        }

        private void ReceiverManager_Thread()
        {
            byte[] dataRx;
            int lengthRx = 0;

            byte[] PacketData = new byte[5 * _MaxSizePacket + 3];
            int PacketLen = 0;

            int CountTimeDetectPacket = 0;

            while (true)
            {
                if (Uart2Com.IsOpen == true)
                {
                    dataRx = new byte[5 * _MaxSizePacket + 3];
                    if (E_OK == Recv(ref dataRx, ref lengthRx))
                    {
                        for (int CountByte = 0; CountByte < lengthRx; CountByte++)
                        {
                            PacketData[PacketLen + CountByte] = dataRx[CountByte];
                        }
                        PacketLen += lengthRx;
                        //CountTimeDetectPacket = 0;
                    }
                    else
                    {
                        if (++CountTimeDetectPacket == TIME_DETECT_PACKET)
                        {
                            CountTimeDetectPacket = 0;
                            if (PacketLen != 0)
                            {
                                OnNotifyRecvPacket(PacketData, PacketLen);
                                PacketLen = 0;
                            }
                        }

                    }
                }
                Thread.Sleep(1);
            }
        }
        private void Notify_RecvPacket(object sender, RecvPacket e)
        {
            if (_FindComDataRx_Length == 0)
            {
                // Array.Copy(e.DataRecv, _FindComDataRx, e.Length);
                _FindComDataRx = e.DataRecv;
                _FindComDataRx_Length = e.Length;
            }
        }
        public void Setup_InfoComport(int Baudrate, int Databit, StopBits StopBit, Parity Par)
        {
            _Baudrate = Baudrate;
            _Databit = Databit;
            _StopBit = StopBit;
            _Par = Par;
        }
        public void FindComPort(byte[] PingPacket, int lengthPing, byte[] PongPacket, int lengthPong, int CyclicPeriod, bool AutoConnect_Stt)
        {
            string[] ListPort = SerialPort.GetPortNames();
            int ret = ComPort.E_NOT_OK;

            _PingPacket = PingPacket;
            _PingPacket_Length = lengthPing;
            Array.Copy(PingPacket, _PingPacket, lengthPing);

            _PongPacket = PongPacket;
            _PongPacket_Length = lengthPong;
            Array.Copy(PongPacket, _PongPacket, lengthPong);

            _CyclicPeriod = CyclicPeriod;
            _AutoConnect = AutoConnect_Stt;
            _ChannelSendPing = 0;

            if (_CyclicPeriod <= MINTIME_SENDPING)
            {
                _CyclicPeriod = MINTIME_SENDPING;
            }

            try
            {
                if (FindComPort_trd != null)
                {
                    FindComPort_trd.Abort();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FindComPort: " + ex.Message);
            }

            for (int countChannel = 0; countChannel < MAX_BUFFER_SEND; countChannel++)
            {
                SetupChannel(countChannel, false);
            }

            FindComPort_trd = new Thread(new ThreadStart(this.FindComPort_Thread));
            FindComPort_trd.IsBackground = true;
            FindComPort_trd.Start();
        }
        private void FindComPort_Thread()
        {
            int ret = ComPort.E_NOT_OK;
            string[] ListPort = SerialPort.GetPortNames();

            byte[] dataRx = new byte[2 * _MaxSizePacket + 3];

            int CountComPort = 0;
            int FindComState = CONNECT_COM_STATE;
            int StateOld = CONNECT_COM_STATE;
            int CountTimeoutWaitPong = 0;

            while (true)
            {

                if (FindComState == CONNECT_COM_STATE)
                {
                    if (Uart2Com.IsOpen == true)
                    {
                        Close();
                    }

                    try
                    {
                        ret = SetupComPort(ListPort[CountComPort], _Baudrate, _Databit, _StopBit, _Par);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error_Comport NULL.... " + ex.Message);
                    }

                    if (ret == E_OK)
                    {
                        _FindComDataRx = new byte[2 * _MaxSizePacket + 3];
                        NotifyRecvPacket += Notify_RecvPacket;
                        FindComState = CONNECT_SENDPING_STATE;
                    }
                    else
                    {
                        CountComPort++;
                    }

                    if (CountComPort >= ListPort.Length)
                    {
                        CountComPort = 0;
                        ListPort = SerialPort.GetPortNames();
                        if (_AutoConnect == false)
                        {
                            SetupChannel(_ChannelSendPing, false);
                            OnNotifyStatusConnection(E_NOT_OK, "");
                            NotifyRecvPacket -= Notify_RecvPacket;
                            FindComPort_trd.Abort();
                        }
                    }
                }
                else if (FindComState == CONNECT_SENDPING_STATE)
                {

                    ret = SendPacketPing(_PingPacket, _PingPacket_Length, true, _CyclicPeriod);
                    if (ret == E_OK)
                    {
                        StateOld = CONNECT_SENDPING_STATE;
                        FindComState = CONNECT_WAITPONG_STATE;
                    }
                    else
                    {
                        FindComState = CONNECT_COM_STATE;
                        CountComPort++;
                    }

                }
                else if (FindComState == CONNECT_WAITPONG_STATE)
                {
                    ret = E_NOT_OK;
                    if (_FindComDataRx_Length != 0)
                    {
                        ret = CompareByteArray(_FindComDataRx, _FindComDataRx_Length, _PongPacket, _PongPacket_Length);

                        if (ret == E_OK)
                        {
                            if (StateOld == CONNECT_SENDPING_STATE)
                            {
                                SetupChannel(_ChannelSendPing, false);
                                OnNotifyStatusConnection(E_OK, ListPort[CountComPort]);
                            }

                            if (_AutoConnect == true)
                            {
                                StateOld = CONNECT_WAITPONG_STATE;
                            }
                            else
                            {
                                StateOld = CONNECT_COM_STATE;
                                NotifyRecvPacket -= Notify_RecvPacket;
                                FindComPort_trd.Abort();
                            }


                            CountTimeoutWaitPong = 0;
                        }
                        else if (StateOld == CONNECT_WAITPONG_STATE)
                        {
                            if (_AutoConnect == false)
                            {
                                StateOld = CONNECT_COM_STATE;
                                NotifyRecvPacket -= Notify_RecvPacket;
                                FindComPort_trd.Abort();
                            }
                            CountTimeoutWaitPong = 0;
                        }
                        else
                        {
                            if (++CountTimeoutWaitPong >= 10 * (_CyclicPeriod + 1))
                            {
                                CountTimeoutWaitPong = 0;
                                if (StateOld == CONNECT_WAITPONG_STATE)
                                {
                                    SetupChannel(_ChannelSendPing, false);
                                    OnNotifyStatusConnection(E_NOT_OK, "");
                                }
                                CountComPort++;
                                FindComState = CONNECT_COM_STATE;
                            }

                        }

                        _FindComDataRx_Length = 0;
                    }
                    else
                    {
                        if (CountTimeoutWaitPong % 10 == 0 && CountTimeoutWaitPong != 0)
                        {
                            SendPacketPing(_PingPacket, _PingPacket_Length, true, _CyclicPeriod);
                        }

                        if (++CountTimeoutWaitPong >= (5 * _CyclicPeriod + 1))
                        {
                            CountTimeoutWaitPong = 0;
                            if (StateOld == CONNECT_WAITPONG_STATE)
                            {
                                SetupChannel(_ChannelSendPing, false, _CyclicPeriod);
                                OnNotifyStatusConnection(E_NOT_OK, "");
                            }
                            CountComPort++;
                            FindComState = CONNECT_COM_STATE;
                        }
                    }

                }

                Thread.Sleep(1);
            }
        }
        public int CompareByteArray(byte[] src, int len_src, byte[] des, int len_des)
        {
            int ret = E_NOT_OK;
            int CountPos = 0;
            int CountByte = 0;

            if (len_src < len_des)
            {
                return E_NOT_OK;
            }

            for (CountPos = 0; CountPos < (len_src - len_des + 1); CountPos++)
            {
                for (CountByte = 0; CountByte < len_des; CountByte++)
                {
                    if (src[CountPos + CountByte] != des[CountByte])
                    {
                        break;
                    }
                }

                if (CountByte == len_des)
                {
                    ret = E_OK;
                    break;
                }
            }

            return ret;
        }
        private void SendManager_Thread()
        {
            int ret = E_NOT_OK;
            byte[] DataSend;
            int[] CountCyclic = new int[MAX_BUFFER_SEND];


            while (true)
            {
                if (LengthSend[CountBuffer] != 0)
                {
                    DataSend = new byte[2048];
                    for (int Countbyte = 0; Countbyte < LengthSend[CountBuffer]; Countbyte++)
                    {
                        DataSend[Countbyte] = tbBufferSend[CountBuffer, Countbyte];
                    }

                    try
                    {
                        if (++CountCyclic[CountBuffer] >= CyclicPeriod[CountBuffer])
                        {
                            ret = Send(DataSend, LengthSend[CountBuffer]);
                            CountCyclic[CountBuffer] = 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }

                    if (CountCyclic[CountBuffer] == 0)
                    {
                        if (ret == E_OK)
                        {
                            OnNotifySendPacket(E_OK, DataSend, LengthSend[CountBuffer]);
                            if (CyclicStatus[CountBuffer] == false)
                            {
                                LengthSend[CountBuffer] = 0;
                            }
                            Thread.Sleep(1);

                        }
                        else if (ret == E_NOT_OK)
                        {
                            if (++CountNumSend[CountBuffer] >= MAX_TRY_SEND_NUMBER)
                            {
                                OnNotifySendPacket(E_NOT_OK, DataSend, LengthSend[CountBuffer]);
                                CountNumSend[CountBuffer] = 0;
                                LengthSend[CountBuffer] = 0;
                            }
                        }
                    }
                }

                if (++CountBuffer >= MAX_BUFFER_SEND)
                {
                    CountBuffer = 0;
                    Thread.Sleep(1);
                }
                //Thread.Sleep(1);
            }
        }

        private event EventHandler<StatusSendPacket> _NotifySendPacket;
        public event EventHandler<StatusSendPacket> NotifySendPacket
        {
            add
            {
                _NotifySendPacket += value;
            }
            remove
            {
                _NotifySendPacket -= value;
            }
        }

        private event EventHandler<RecvPacket> _NotifyRecvPacket;
        public event EventHandler<RecvPacket> NotifyRecvPacket
        {
            add
            {
                _NotifyRecvPacket += value;
            }
            remove
            {
                _NotifyRecvPacket -= value;
            }
        }
        private event EventHandler<NotifyStatusConnection> _StatusConnection;
        public event EventHandler<NotifyStatusConnection> StatusConnection
        {
            add
            {
                _StatusConnection += value;
            }
            remove
            {
                _StatusConnection -= value;
            }
        }

        protected virtual void OnNotifySendPacket(int status, byte[] datasend, int length)
        {
            if (_NotifySendPacket != null)
            {
                _NotifySendPacket(this, new StatusSendPacket(status, datasend, length));
            }
        }
        protected virtual void OnNotifyRecvPacket(byte[] datarecv, int length)
        {
            if (_NotifyRecvPacket != null)
            {
                _NotifyRecvPacket(this, new RecvPacket(datarecv, length));
            }
        }
        protected virtual void OnNotifyStatusConnection(int status, string nameCom)
        {
            if (_StatusConnection != null)
            {
                _StatusConnection(this, new NotifyStatusConnection(status, nameCom));
            }
        }
    }
    public class StatusSendPacket : EventArgs
    {
        public int Status = 0;
        public byte[] DataSend;
        public int Length;
        public StatusSendPacket(int status, byte[] datasend, int length)
        {
            Status = status;
            DataSend = datasend;
            Length = length;
        }
    }
    public class RecvPacket : EventArgs
    {
        public int Status = 0;
        public byte[] DataRecv;
        public int Length;
        public RecvPacket(byte[] datarecv, int length)
        {
            DataRecv = datarecv;
            Length = length;
        }
    }
    public class NotifyStatusConnection : EventArgs
    {
        public int Status = 0;
        public string NameCom = "";

        public NotifyStatusConnection(int status, string nameCom)
        {
            Status = status;
            NameCom = nameCom;
        }
    }
}

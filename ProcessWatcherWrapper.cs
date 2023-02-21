﻿using System;
using System.Diagnostics;

namespace Display
{
    public class ProcessWatcherWrapper
    {
        public void Execute(string programA, string programB)
        {
            var notepadPlusPlus = new Process();
            notepadPlusPlus = Process.GetCurrentProcess();
            //notepadPlusPlus.StartInfo.FileName = programA;
            //notepadPlusPlus = Process.GetProcessesByName(programA)[0];
            //notepadPlusPlus.Start();

            Action callBack = new Action(() =>
            {
                var notepad = new Process();
                notepad.StartInfo.FileName = programB;
                notepad.Start();
            });

            var processWatcher = new ProcessWatcher(callBack, notepadPlusPlus.ProcessName, 2000);
            processWatcher.StartWatch();
        }
    }
}

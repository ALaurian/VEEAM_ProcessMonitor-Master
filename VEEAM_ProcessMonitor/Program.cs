/*This command line utility expects three arguments: a process name, its
maximum lifetime (in minutes) and a monitoring frequency (in minutes, as
well). When you run the program, it starts monitoring processes with the
frequency specified. If a process of interest lives longer than the allowed
duration, the utility kills the process and adds the corresponding record to the
log. When no process exists at any given moment, the utility continues
monitoring (new processes might appear later). The utility stops when a
special keyboard button is pressed (say, q).

Here is the example: monitor.exe notepad 5 1 – every other minute, the
program verifies if a notepad process lives longer than 5 minutes, and if it
does, the program kills the process.*/

using System.Diagnostics;
using ThreadState = System.Diagnostics.ThreadState;

//This was written in .NET7, so the console app is a bit different from previous .NET versions.
//There was also no reason for me to use classes or to write the logic separately from the main method since 
//the program is so small and the instructions did not specify any requirements for the program to be 
//written in a certain way.

//a few variables to fetch the arguments
//Console.WriteLine("Enter the process name: ");
var processName = args[0];
//maxLifetime is calculated as a double, because it is possible to specify fractions of minutes
//Console.WriteLine("Enter the maximum lifetime of the process in (fractional - 0.1) minutes: ");
var maxLifetime = Double.Parse(args[1]);
//frequency is calculated as an integer, because it is not possible to give Thread.Sleep a double
//Console.WriteLine("Enter the monitoring frequency in (fractional - 0.1) minutes: ");
var monitoringFrequency = (int)(Double.Parse(args[2]) * 1000);
var dictionaryOfKillings = new Dictionary<string, DateTime>(); //honestly this test is making me seem like a serial killer
var index = 0;

//Cancellation token for the while loop
var cts = new CancellationTokenSource();

//I created a thread so that I could know when the user pressed a key to stop the program
//if I had just wrapped this in a while(true) loop, the program would check for the keypress on every iteration
//every 'monitoringFrequency' milliseconds, which is a waste of everyone's time.
//I could have used a timer, but I wanted to use threads.
var thread = new Thread(() =>
{
    while (!cts.IsCancellationRequested)
    {
        //I get the first process that matches the name, the test did not mention an array of processes
        var process = Process.GetProcessesByName(processName).FirstOrDefault();
        
        if (process != null)
        {
            //fetch the process start time
            var processStartTime = process.StartTime;
            //calculates how many minutes the process has been running
            var processLifetime = (DateTime.Now - processStartTime).TotalMinutes;

            //checks if the process has been running for more than the allowed time
            if (processLifetime > maxLifetime)
            {
                //logs all the information in the console
                Console.WriteLine(process.ProcessName + " was killed at " + DateTime.Now.ToString("yy-MMM-dd HH:mm:ss"));
                //logs all the information in the dictionary with an index identifier (dictionaries do not allow duplicate keys)
                dictionaryOfKillings.Add(index + "_" + process.ProcessName, DateTime.Now);
                //increments the index
                index++;
                //...kills the process :(
                process.Kill();
            }
        }
    
        //Without this monitoringFrequency this application would use a lot of CPU %, a very low number would also increase
        //cpu usage
        Thread.Sleep(monitoringFrequency);
    }
});

//starts the thread
thread.Start();

//this is the keypress handler
while (true)
{
    //register key press on Q
    if (Console.ReadKey().Key == ConsoleKey.Q)
    {
        cts.Cancel();
        while (thread.ThreadState != System.Threading.ThreadState.Stopped)
        {
            
        }
        
        //prints the dictionary of killings
        Console.WriteLine("\nThe following processes were killed:\n");
        foreach (var process in dictionaryOfKillings)
        {
            Console.WriteLine(process.Key + " at " + process.Value.ToString("yy-MMM-dd HH:mm:ss"));
        }
        
        //stops the program
        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
        break;
    }
}


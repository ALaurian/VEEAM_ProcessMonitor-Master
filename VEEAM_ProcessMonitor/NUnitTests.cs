using System.Diagnostics;
using NUnit.Framework;
using ThreadState = System.Threading.ThreadState;

namespace VEEAM_ProcessMonitor;

//I packaged the NUnit Tests here instead of a separate project, installing the Microsoft.NET.Test.Sdk NuGet package
//will make the original project unrunnable due to SDK changing, so I decided to keep them here for this test.
[TestFixture]
public class NUnitTests
{
    //This test is a perequisite to other tests, some will fail if Notepad is not open.
    [Test]
    [TestCase("notepad.exe")]
    public void OpenNotepad(string processName)
    {
        var process = Process.Start(processName);
        Assert.That(process != null);
    }

    [Test]
    [TestCase("notepad")]
    public void FoundProcesses(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        
        Assert.That(processes.Any());
    }

    [Test]
    [TestCase("notepad")]
    public void KillProcesses(string processName)
    {
        var processes = Process.GetProcessesByName(processName);

        if (processes.Any())
        {
            foreach (var process in processes)
            {
                process?.Kill();
                Assert.That(process.HasExited);
            } 
        }

    }

    [Test]
    [TestCase(null)]
    public void IsThreadRunning(CancellationTokenSource condition)
    {
        condition = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            while (!condition.IsCancellationRequested)
            {
            }
        });
        thread.Start();
        Assert.That(thread.ThreadState == ThreadState.Running);
        condition.Cancel();
    }

    [Test]
    [TestCase(null)]
    public void IsThreadStopped(CancellationTokenSource condition)
    {
        condition = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            while (!condition.IsCancellationRequested)
            {
            }
        });
        thread.Start();
        condition.Cancel();

        while (thread.ThreadState != ThreadState.Stopped)
        {
        }

        Assert.That(thread.ThreadState == ThreadState.Stopped);
    }
}
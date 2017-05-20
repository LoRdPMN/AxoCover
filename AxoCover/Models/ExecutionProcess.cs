﻿using AxoCover.Common.Events;
using AxoCover.Common.Extensions;
using AxoCover.Common.Models;
using AxoCover.Common.ProcessHost;
using AxoCover.Common.Runner;
using AxoCover.Common.Settings;
using AxoCover.Models.Extensions;
using System;
using System.Collections.Generic;

namespace AxoCover.Models
{
  public class ExecutionProcess : TestProcess<ITestExecutionService>, ITestExecutionMonitor
  {
    public event EventHandler<EventArgs<string>> MessageReceived;
    public event EventHandler<EventArgs<TestCase>> TestStarted;
    public event EventHandler<EventArgs<TestCase>> TestEnded;
    public event EventHandler<EventArgs<TestResult>> TestResult;
    public event EventHandler DebuggerAttached;

    private ExecutionProcess(IHostProcessInfo hostProcess, string[] testPlatformAssemblies) :
      base(hostProcess.Embed(new ServiceProcessInfo(RunnerMode.Execution, testPlatformAssemblies))) { }
    
    public static ExecutionProcess Create(string[] testPlatformAssemblies, IHostProcessInfo hostProcess = null, TestPlatform testPlatform = TestPlatform.x86)
    {
      hostProcess = hostProcess.Embed(new PlatformProcessInfo(testPlatform)) as IHostProcessInfo;

      var executionProcess = new ExecutionProcess(hostProcess, testPlatformAssemblies);

      if (executionProcess.TestService == null)
      {
        throw new Exception("Could not create service.");
      }
      else
      {
        return executionProcess;
      }
    }
    
    void ITestExecutionMonitor.RecordMessage(TestMessageLevel testMessageLevel, string message)
    {
      var text = testMessageLevel.GetShortName() + " " + message;
      MessageReceived?.Invoke(this, new EventArgs<string>(text));
    }

    void ITestExecutionMonitor.RecordStart(TestCase testCase)
    {
      TestStarted?.Invoke(this, new EventArgs<TestCase>(testCase));
    }

    void ITestExecutionMonitor.RecordEnd(TestCase testCase, TestOutcome outcome)
    {
      TestEnded?.Invoke(this, new EventArgs<TestCase>(testCase));
    }

    void ITestExecutionMonitor.RecordResult(TestResult testResult)
    {
      TestResult?.Invoke(this, new EventArgs<TestResult>(testResult));
    }

    void ITestExecutionMonitor.RecordDebuggerStatus(bool isAttached)
    {
      if (isAttached)
      {
        DebuggerAttached?.Invoke(this, EventArgs.Empty);
      }
    }

    public void RunTests(IEnumerable<TestCase> testCases, TestExecutionOptions options)
    {
      TestService.RunTests(testCases, options);
    }
  }
}

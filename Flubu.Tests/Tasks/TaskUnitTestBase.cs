﻿using FlubuCore.Context;
using FlubuCore.Context.FluentInterface.Interfaces;
using FlubuCore.Tasks.Process;
using Moq;

namespace Flubu.Tests.Tasks
{
    public abstract class TaskUnitTestBase
    {
        protected TaskUnitTestBase(MockBehavior propertiesMockBehavior = MockBehavior.Strict)
        {
            Tasks = new Mock<ITaskFluentInterface>();
            CoreTasks = new Mock<ICoreTaskFluentInterface>();
            Context = new Mock<ITaskContextInternal>();
            Properties = new Mock<IBuildPropertiesSession>(propertiesMockBehavior);
            Context.Setup(x => x.Properties).Returns(Properties.Object);
            Context.Setup(x => x.Tasks()).Returns(Tasks.Object);
            Context.Setup(x => x.CoreTasks()).Returns(CoreTasks.Object);

            RunProgramTask = new Mock<IRunProgramTask>();
            RunProgramTask.Setup(x => x.WithArguments(It.IsAny<string>())).Returns(RunProgramTask.Object);
            RunProgramTask.Setup(x => x.WithArguments(It.IsAny<string[]>())).Returns(RunProgramTask.Object);
            RunProgramTask.Setup(x => x.WorkingFolder(It.IsAny<string>())).Returns(RunProgramTask.Object);
            RunProgramTask.Setup(x => x.CaptureErrorOutput()).Returns(RunProgramTask.Object);
            RunProgramTask.Setup(x => x.CaptureOutput()).Returns(RunProgramTask.Object);
        }

        protected Mock<ITaskContextInternal> Context { get; set; }

        protected Mock<IBuildPropertiesSession> Properties { get; set; }

        protected Mock<ITaskFluentInterface> Tasks { get; set; }

        protected Mock<ICoreTaskFluentInterface> CoreTasks { get; set; }

        protected Mock<IRunProgramTask> RunProgramTask { get; set; }
    }
}

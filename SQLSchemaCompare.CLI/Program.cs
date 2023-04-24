namespace TiCodeX.SQLSchemaCompare.CLI
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using CommandLine;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Repository;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
    using TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders;
    using TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseUtilities;
    using TiCodeX.SQLSchemaCompare.Infrastructure.Repository;
    using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
    using TiCodeX.SQLSchemaCompare.Services;

    /// <summary>
    /// SQLSchemaCompare CLI application
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Gets or sets container
        /// </summary>
        private static IContainer Container { get; set; }

        /// <summary>
        /// Entry point of the SQLSchemaCompare UI application
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>The exit code</returns>
        public static int Main(string[] args)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ProjectService>().As<IProjectService>().SingleInstance();
            builder.RegisterType<TaskService>().As<ITaskService>().SingleInstance();
            builder.RegisterType<CipherService>().As<ICipherService>().SingleInstance();

            // Repository
            builder.RegisterType<ProjectRepository>().As<IProjectRepository>();

            // Service
            builder.RegisterType<DatabaseService>().As<IDatabaseService>();
            builder.RegisterType<DatabaseCompareService>().As<IDatabaseCompareService>();

            // Factory
            builder.RegisterType<DatabaseProviderFactory>().As<IDatabaseProviderFactory>();
            builder.RegisterType<DatabaseScripterFactory>().As<IDatabaseScripterFactory>();

            // Utilities
            builder.RegisterType<DatabaseMapper>().As<IDatabaseMapper>();
            builder.RegisterType<DatabaseFilter>().As<IDatabaseFilter>();

            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>();

            Container = builder.Build();

            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(RunAndReturnExitCode, _ => 1);
        }

        /// <summary>
        /// Run and return exit code.
        /// </summary>
        /// <param name="options">The options</param>
        /// <returns>The exit code</returns>
        private static int RunAndReturnExitCode(Options options)
        {
            using var scope = Container.BeginLifetimeScope();

            var projectService = scope.Resolve<IProjectService>();
            projectService.LoadProject(options.ProjectFile);

            var taskService = scope.Resolve<ITaskService>();

            var databaseCompareService = scope.Resolve<IDatabaseCompareService>();
            databaseCompareService.StartCompare();

            while (!taskService.CurrentTaskInfos.All(x => x.Status == TaskStatus.RanToCompletion ||
                                                          x.Status == TaskStatus.Faulted ||
                                                          x.Status == TaskStatus.Canceled))
            {
                Thread.Sleep(200);
            }

            if (taskService.CurrentTaskInfos.Any(x => x.Status == TaskStatus.Faulted ||
                                                      x.Status == TaskStatus.Canceled))
            {
                var exception = taskService.CurrentTaskInfos.FirstOrDefault(x => x.Status == TaskStatus.Faulted ||
                                                                                 x.Status == TaskStatus.Canceled)?.Exception;
                if (exception != null)
                {
                    throw exception;
                }

                throw new InvalidOperationException("Unknown error during compare task");
            }

            File.WriteAllText(options.OutputFile, projectService.Project.Result.FullAlterScript);

            return 0;
        }
    }
}

using System;
using System.Diagnostics;

namespace SonarProcedures
{
   internal static class Program
    {
        static void Main(string[] args)
        {
            //RunBaselineAnalysis();
            RunAfterFixesAnalysis();
        }

        static void RunBaselineAnalysis()
        {
            Console.WriteLine("Running baseline analysis in ./baseline ...");

            var hostUrl = Environment.GetEnvironmentVariable("SONAR_HOST_URL");
            var token = Environment.GetEnvironmentVariable("SONAR_TOKEN");
            if (string.IsNullOrWhiteSpace(hostUrl) || string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Set SONAR_HOST_URL and SONAR_TOKEN env vars first.");

            var projectRoot = Directory.GetCurrentDirectory();
            Console.WriteLine(projectRoot);
            var baselineDir = Path.Combine(projectRoot, "baseline");
            Console.WriteLine(baselineDir);


            var solutionPath = Path.Combine(baselineDir, "StudentExamSystem.sln");
            Console.WriteLine(baselineDir);
            if (!File.Exists(solutionPath))
                throw new FileNotFoundException("Solution not found at", solutionPath);

            Run("dotnet", "tool update --global dotnet-sonarscanner", baselineDir);

            var beginArgs =
                $@"begin /k:""anndjella_student-exam-system"" " +
                $@"/o:""anndjella"" " +
                $@"/v:""baseline-2025-10-22"" " +
                $@"/d:sonar.host.url=""{hostUrl}"" " +
                $@"/d:sonar.login=""{token}"" " +
                @"/d:sonar.cs.opencover.reportsPaths=""**/coverage.opencover.xml"" " +
                @"/d:sonar.coverage.exclusions=""**/*Tests.cs,**/Migrations/**"" " +
                @"/d:sonar.scm.exclusions.disabled=true";

            Run("dotnet-sonarscanner", beginArgs, baselineDir);

            Run("dotnet", @"clean .\StudentExamSystem.sln", baselineDir);
            Run("dotnet", @"restore .\StudentExamSystem.sln", baselineDir);
            Run("dotnet", @"build .\StudentExamSystem.sln -c Release", baselineDir);

            Run("dotnet",
                @"test .\StudentExamSystem.sln -c Release --no-build " +
                @"/p:CollectCoverage=true " +
                @"/p:CoverletOutput=./TestResults/Baseline/coverage.opencover.xml " +
                @"/p:CoverletOutputFormat=opencover",
                baselineDir);

            Run("dotnet-sonarscanner", $@"end /d:sonar.login=""{token}""", baselineDir);

            Console.WriteLine("Baseline analysis done! (folder: baseline)");
        }

        static void RunAfterFixesAnalysis()
        {
            Console.WriteLine("Running main analysis in ./baseline ...");

            var hostUrl = Environment.GetEnvironmentVariable("SONAR_HOST_URL");
            var token = Environment.GetEnvironmentVariable("SONAR_TOKEN");
            if (string.IsNullOrWhiteSpace(hostUrl) || string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Set SONAR_HOST_URL and SONAR_TOKEN env vars first.");

            var projectRoot = Directory.GetCurrentDirectory();
            Console.WriteLine(projectRoot);
          

            var solutionPath = Path.Combine(projectRoot, "StudentExamSystem.sln");
            Console.WriteLine(solutionPath);
            if (!File.Exists(solutionPath))
                throw new FileNotFoundException("Solution not found at", solutionPath);

            Run("dotnet", "tool update --global dotnet-sonarscanner", projectRoot);

            var beginArgs =
                $@"begin /k:""anndjella_student-exam-system"" " +
                $@"/o:""anndjella"" " +
                $@"/v:""after-2025-10-22"" " +
                $@"/d:sonar.host.url=""{hostUrl}"" " +
                $@"/d:sonar.login=""{token}"" " +
                @"/d:sonar.cs.opencover.reportsPaths=""**/coverage.opencover.xml"" " +
                @"/d:sonar.exclusions=""**/Migrations/**,**/Properties/launchSettings.json,**/bin/**,**/obj/**,**/SonarProcedures/**,**/Dockerfile,**/*.Dockerfile,**/docker/**,**/docker-compose*.yml"" " +
                @"/d:sonar.coverage.exclusions=""**/*Tests.cs,**/Migrations/**,**/SonarProcedures/**,**/Dockerfile,**/*.Dockerfile,**/docker/**""";

            Run("dotnet-sonarscanner", beginArgs, projectRoot);

            Run("dotnet", @"clean .\StudentExamSystem.sln", projectRoot);
            Run("dotnet", @"restore .\StudentExamSystem.sln", projectRoot);
            Run("dotnet", @"build .\StudentExamSystem.sln -c Release", projectRoot);

            Run("dotnet",
                @"test .\StudentExamSystem.sln -c Release --no-build " +
                @"/p:CollectCoverage=true " +
                @"/p:CoverletOutput=./TestResults/Baseline/coverage.opencover.xml " +
                @"/p:CoverletOutputFormat=opencover",
                projectRoot);

            Run("dotnet-sonarscanner", $@"end /d:sonar.login=""{token}""", projectRoot);

            Console.WriteLine("Main analysis done!");
        }

        static void Run(string fileName, string arguments, string workingDir)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            p.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
        }
    }
}

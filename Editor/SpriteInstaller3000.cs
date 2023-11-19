using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spritesheet3000.Editor
{
    public class SpriteInstaller3000
    {
        private const string PACKAGE_NAME = "com.am1goo.spritesheet3000";
        private const string PACKAGE_DISPLAY_NAME = "Spritesheet 3000";
        private const string MENU_ITEM_ROOT = PACKAGE_DISPLAY_NAME;

        private static readonly string DoubleLine = $"{Environment.NewLine}{Environment.NewLine}";

        [MenuItem(MENU_ITEM_ROOT + "/Install Extensions/Adobe Photoshop CC")]
        private static void InstallAdobePhotoshopCC()
        {
            var pathToPackage = ResolvePackagePath(PACKAGE_NAME);
            var pathToFolder = Path.Combine(pathToPackage, "Editor/AdobePhotoshop/CC");

            var src = new DirectoryInfo(pathToFolder);
            var installReg = new FileInfo(Path.Combine(src.FullName, "install.reg"));

            var extensionsFolder = @"Common Files\Adobe\CEP\extensions";
            var extensionName = "com.am1goo.photoshop.extension.spritesheet3000";
            var extensionManifest = @"CSXS\manifest.xml";

            var extensionSource = new DirectoryInfo(Path.Combine(src.FullName, extensionName));

            var programFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFiles64di = new DirectoryInfo(Path.Combine(programFiles64, extensionsFolder, extensionName));
            var programFiles64manifest = new FileInfo(Path.Combine(programFiles64di.FullName, extensionManifest));

            var programFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var programFiles32di = new DirectoryInfo(Path.Combine(programFiles32, extensionsFolder, extensionName));
            var programFiles32manifest = new FileInfo(Path.Combine(programFiles32di.FullName, extensionManifest));

            var installer = new Installer();
            installer
                .AddTask(Installer.Priority.Required, new OSCompatibleTask(PlatformID.Win32NT))
                .AddTask(Installer.Priority.Required, new RegEditTask(installReg))
                .AddTask(Installer.Priority.Required, new AnyTask()
                    .AddTask(new OnlyIfError(new FileExistsTask(programFiles32manifest))
                        .AddFallback(new CopyFolderToTask(extensionSource, programFiles32di)))
                    .AddTask(new OnlyIfError(new FileExistsTask(programFiles64manifest))
                        .AddFallback(new CopyFolderToTask(extensionSource, programFiles64di))));

            var report = installer.Run();
            if (!report.success)
            {
                var error = $"Adobe Photoshop's extension wasn't installed:" +
                    $"{DoubleLine}" +
                    $"{report.error}";

                Debug.LogError($"SpriteInstaller3000: {error}");
                EditorUtility.DisplayDialog("Error", error, "Okay");
                return;
            }

            if (report.error != null)
            {
                var error = $"Adobe Photoshop's extension successfully installed, but have some errors:" +
                    $"{DoubleLine}" +
                    $"{report.error}";

                Debug.LogError($"SpriteInstaller3000: {error}");
                EditorUtility.DisplayDialog("Installer", error, "Okay");
                return;
            }

            var log = "Adobe Photoshop's extension successfully installed";

            Debug.Log($"SpriteInstaller3000: {log}");
            EditorUtility.DisplayDialog("Installer", log, "Okay");
        }

        private static string ResolvePackagePath(string packageName)
        {
            var req = UnityEditor.PackageManager.Client.List();
            while (!req.IsCompleted)
            {
                System.Threading.Thread.Sleep(10);
            }

            var found = default(UnityEditor.PackageManager.PackageInfo);
            var collection = req.Result;
            foreach (var package in collection)
            {
                if (package.name != packageName)
                    continue;

                found = package;
                break;
            }

            return found.resolvedPath;
        }

        private class OnlyIfError : Installer.ITask
        {
            private Installer.ITask _task;
            private List<Installer.ITask> _fallbackTasks = new List<Installer.ITask>();

            public OnlyIfError(Installer.ITask task)
            {
                _task = task;
            }

            public OnlyIfError AddFallback(Installer.ITask task)
            {
                _fallbackTasks.Add(task);
                return this;
            }

            public Report Run()
            {
                var report = _task.Run();
                if (report.success)
                    return report;

                foreach (var task in _fallbackTasks)
                {
                    report = task.Run();
                    if (!report.success)
                        return report;
                }

                return Report.Success();
            }
        }

        private class AnyTask : Installer.ITask
        {
            private List<Installer.ITask> _tasks = new List<Installer.ITask>();

            public AnyTask AddTask(Installer.ITask task)
            {
                _tasks.Add(task);
                return this;
            }

            public Report Run()
            {
                var any = false;
                var errors = new List<string>();
                foreach (var task in _tasks)
                {
                    var report = task.Run();
                    if (report.success)
                        any |= true;
                    else
                        errors.Add(report.error);
                }

                if (!any)
                    return Report.Error(string.Join(DoubleLine, errors));

                if (errors.Count > 0)
                    return Report.Success(string.Join(DoubleLine, errors));

                return Report.Success();
            }
        }

        private class OSCompatibleTask : Installer.ITask
        {
            private PlatformID[] _ids;

            public OSCompatibleTask(params PlatformID[] ids)
            {
                _ids = ids;
            }

            public Report Run()
            {
                var platformId = Environment.OSVersion.Platform;
                if (Array.Exists(_ids, x => x == platformId))
                {
                    return Report.Success();
                }
                else
                {
                    return Report.Error($"Unsupported OS {platformId}");
                }
            }
        }

        private class FileExistsTask : Installer.ITask
        {
            private FileInfo _fi;

            public FileExistsTask(FileInfo fi)
            {
                _fi = fi;
            }

            public Report Run()
            {
                if (!_fi.Exists)
                    return Report.Error($"File {_fi.FullName} doesn't exists");

                return Report.Success();
            }
        }

        private class RegEditTask : Installer.ITask
        {
            private FileInfo _fi;

            public RegEditTask(FileInfo fi)
            {
                _fi = fi;
            }

            public Report Run()
            {
                if (!_fi.Exists)
                    return Report.Error($"Cannot use registry's install file at path {_fi.FullName}, because it doens't exists.");

                try
                {
                    using (var proc = System.Diagnostics.Process.Start("regedit.exe", $"/s {_fi.FullName}"))
                    {
                        proc.WaitForExit();
                        if (proc.ExitCode != 0)
                            return Report.Error("Error while using registry's install file");

                        return Report.Success();
                    }
                }
                catch (Exception ex)
                {
                    return Report.Error(ex);
                }
            }
        }

        private class CopyFolderToTask : Installer.ITask
        {
            private DirectoryInfo _src;
            private DirectoryInfo _dest;

            public CopyFolderToTask(DirectoryInfo src, DirectoryInfo dest)
            {
                _src = src;
                _dest = dest;
            }

            public Report Run()
            {
                if (!_src.Exists)
                    return Report.Error($"Cannot copy extension files from {_src.FullName}, because it doens't exists.");

                try
                {
                    Copy(_src, _dest);
                }
                catch (UnauthorizedAccessException _)
                {
                    return Report.Error($"Run this operation with Administrative Rights or copy extension files manually from directory {_src} to directory {_dest}");
                }
                catch (Exception ex)
                {
                    return Report.Error(ex);
                }
                return Report.Success();
            }

            private void Copy(DirectoryInfo src, DirectoryInfo dest)
            {
                if (!dest.Exists)
                    dest.Create();

                foreach (var file in src.GetFiles())
                {
                    File.Copy(file.FullName, Path.Combine(dest.FullName, Path.GetFileName(file.FullName)), overwrite: true);
                }

                foreach (var directory in src.GetDirectories())
                {
                    var next = new DirectoryInfo(Path.Combine(dest.FullName, Path.GetFileName(directory.FullName)));
                    Copy(directory, next);
                }
            }
        }

        private class Installer
        {
            private Queue<Entry> _entries = new Queue<Entry>();

            public Report Run()
            {
                var success = true;
                var errors = new List<string>();
                while (_entries.Count > 0)
                {
                    if (!success)
                        break;

                    var entry = _entries.Dequeue();
                    var report = entry.task.Run();
                    if (report.success)
                        continue;

                    errors.Add(report.error);
                    switch (entry.priority)
                    {
                        case Priority.Required:
                            success = false;
                            break;

                        case Priority.Optional:
                            //do nothing
                            break;
                    }
                }

                if (!success)
                    return Report.Error(string.Join(DoubleLine, errors));

                if (errors.Count > 0)
                    return Report.Success(string.Join(DoubleLine, errors));

                return Report.Success();
            }

            public Installer AddTask(Priority priority, ITask task)
            {
                _entries.Enqueue(new Entry(priority, task));
                return this;
            }

            private class Entry
            {
                private Priority _priority;
                public Priority priority => _priority;

                private ITask _task;
                public ITask task => _task;

                public Entry(Priority priority, ITask task)
                {
                    _priority = priority;
                    _task = task;
                }
            }

            public interface ITask
            {
                Report Run();
            }

            public enum Priority
            {
                Required = 0,
                Optional = 1,
            }
        }

        private struct Report
        {
            public bool success;
            public string error;

            public static Report Success()
            {
                return Success(null);
            }

            public static Report Success(string error)
            {
                return new Report
                {
                    success = true,
                    error = error,
                };
            }

            public static Report Error(Exception ex)
            {
                return Error(ex.ToString());
            }

            public static Report Error(string error)
            {
                return new Report
                {
                    success = false,
                    error = error,
                };
            }
        }
    }
}

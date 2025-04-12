using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ProcessExplorer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Thread processThread = new Thread(LoadProcessTree);
        processThread.IsBackground = true;
        processThread.Start();
    }

    private void LoadProcessTree()
    {
        var processes = new Dictionary<int, ProcessInfo>();
        var rootProcesses = new List<ProcessInfo>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                processes[process.Id] = new ProcessInfo
                {
                    ProcessId = process.Id,
                    ParentProcessId = process.ParentProcessId(),
                    Name = process.ProcessName,
                    MemoryUsageMB = process.WorkingSet64 / (1024 * 1024)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // Parent-child relationships
        foreach (var process in processes.Values)
        {
            if (processes.TryGetValue(process.ParentProcessId, out var parentProcess))
            {
                parentProcess.Children.Add(process);
            }
            else
            {
                rootProcesses.Add(process);
            }
        }

        // Updating Tree on the UI main thread
        Dispatcher.Invoke(() =>
        {
            ProcessTreeView.Items.Clear();
            foreach (var rootProcess in rootProcesses)
            {
                ProcessTreeView.Items.Add(CreateTreeViewItem(rootProcess));
            }
        });
    }

    private TreeViewItem CreateTreeViewItem(ProcessInfo info)
    {
        var item = new TreeViewItem
        {
            Header = $"{info.Name} (PID: {info.ProcessId})",
            Tag = info
        };

        foreach (var child in info.Children)
        {
            item.Items.Add(CreateTreeViewItem(child));
        }

        return item;
    }

    private void ProcessTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (ProcessTreeView.SelectedItem is TreeViewItem selectedItem &&
            selectedItem.Tag is ProcessInfo info)
        {
            ProcessDetailsTextBox.Text =
                $"Process Name: {info.Name}\n" +
                $"PID: {info.ProcessId}\n" +
                $"Parent PID: {info.ParentProcessId}\n" +
                $"Memory / Usage: {info.MemoryUsageMB}MB\n";
        }
    }
}

public static class ProcessExtensions
{
    public static int ParentProcessId(this Process process)
    {
        try
        {
            using (var query = new PerformanceCounter("Process", "Creating Process ID", process.ProcessName))
            {
                return (int)query.RawValue;
            }
        }
        catch
        {
            return 0;
        }
    }
}

public class ProcessInfo
{
    public int ProcessId { get; set; }
    public int ParentProcessId { get; set; }
    public string Name { get; set; }
    public long MemoryUsageMB { get; set; }

    public List<ProcessInfo> Children { get; set; } = new List<ProcessInfo>();
}
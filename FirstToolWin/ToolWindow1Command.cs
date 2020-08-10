﻿using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.Windows.Forms;

namespace FirstToolWin
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ToolWindow1Command
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0d6e333e-c0fb-46f6-8176-2b5d8463a0d3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// For FirstToolWinPackage.vsct - makes the commands available in code
        public const string guidFirstToolWindowPackageCmdSet = "00000000-0000-0000-0000-0000";  // get the GUID from the .vsct file
        public const uint cmdidWindowsMedia = 0x100;
        public const int cmdidWindowsMediaOpen = 0x132;
        public const int ToolbarID = 0x1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Command"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ToolWindow1Command(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ToolWindow1Command Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ToolWindow1Command's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ToolWindow1Command(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(ToolWindow1), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            // Create the handles for the toolbar command.
            var mcs = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            var toolbarbtnCmdID = new CommandID(new Guid(ToolWindow1Command.guidFirstToolWindowPackageCmdSet),
                ToolWindow1Command.cmdidWindowsMediaOpen);
            var menuItem = new MenuCommand(new EventHandler(ButtonHandler), toolbarbtnCmdID);
            mcs.AddCommand(menuItem);
        }
    }
}

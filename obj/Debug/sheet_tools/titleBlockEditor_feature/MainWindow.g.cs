﻿#pragma checksum "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "F57799D2544879CFD6242EC9AE85460785FCBD67A4122924E7B21385A738F582"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using bimkit.sheet_tools.titleBlockEditor_feature;


namespace bimkit.sheet_tools.titleBlockEditor_feature {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 21 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image TitleBarImage;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox SheetListBox;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel GroupButtonsPanel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/bimkit;component/sheet_tools/titleblockeditor_feature/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 13 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
            ((System.Windows.Controls.Border)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.TitleBar_MouseDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.TitleBarImage = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            
            #line 22 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_1);
            
            #line default
            #line hidden
            return;
            case 4:
            this.SheetListBox = ((System.Windows.Controls.ListBox)(target));
            return;
            case 5:
            this.GroupButtonsPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 6:
            
            #line 51 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SelectAll_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 52 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SelectNone_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 53 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ApplyChanges_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 56 "..\..\..\..\sheet_tools\titleBlockEditor_feature\MainWindow.xaml"
            ((System.Windows.Documents.Hyperlink)(target)).RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


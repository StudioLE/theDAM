﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Controls;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Newtonsoft.Json;

namespace theDAM.AnalyzeGraphs
{
    /// <summary>
    /// Interaction logic for AnalyzeGraphs.xaml
    /// </summary>
    public partial class AnalyzeGraphs : Window
    {
        public class TheDamGraph
        {
            public string GraphName { get; set; }
            public string GraphPurpose { get; set; }
            public int NodeCount { get; set; }
        }

        private List<string> _filePaths;
        private Dictionary<string, string> _categoryDictionary = new Dictionary<string, string>();
        public AnalyzeGraphs()
        {
            LoadCategorizationGraphs();
            InitializeComponent();
        }

        private void ButtonDirectory_Click(object sender, RoutedEventArgs e)
        {       
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    SearchOption searchOption = SearchOption.TopDirectoryOnly;
                    if (this.CheckBoxSubdirectories.IsChecked.Value)
                    {
                        searchOption = SearchOption.AllDirectories;
                    }

                    TextBoxDirectory.Text = fbd.SelectedPath;

                    _filePaths = Directory.EnumerateFiles(fbd.SelectedPath, "*.*", searchOption)
                        .Where(s => s.EndsWith(".dyn")).ToList();
                    PackLists();
                }
            }
        }

        private void PackLists()
        {
            //grid view to add the dynamo info to the list
            GridView grid = new GridView();
            //column to contain graph names
            GridViewColumn col0 = new GridViewColumn();
            col0.Width = 200;
            col0.Header = "Graph Name";
            col0.DisplayMemberBinding = new System.Windows.Data.Binding("GraphName");
            grid.Columns.Add(col0);
            //column to add node counts to
            GridViewColumn col1 = new GridViewColumn();
            col1.Width = 200;
            col1.Header = "Graph Purpose";
            col1.DisplayMemberBinding = new System.Windows.Data.Binding("GraphPurpose");
            grid.Columns.Add(col1);
            //column to add node counts to
            GridViewColumn col2 = new GridViewColumn();
            col2.Width = 200;
            col2.Header = "Node Count";
            col2.DisplayMemberBinding = new System.Windows.Data.Binding("NodeCount");
            grid.Columns.Add(col2);

            //bind the list view to the grid
            this.ListViewDynamoInfo.View = grid;
            //iterate through the file paths to get the info
            foreach (string file in _filePaths)
            {
                WorkspaceModel workspaceModel = WorkspaceFromJSON(file);

                string graphType = string.Empty;
                foreach (NodeModel node in workspaceModel.Nodes)
                {
                    string returnValue;
                    _categoryDictionary.TryGetValue(node.Name, out returnValue);
                    graphType = returnValue;
                }

                this.ListViewDynamoInfo.Items.Add(new TheDamGraph()
                {
                    GraphName = workspaceModel.Name,
                    GraphPurpose = graphType,
                    NodeCount = workspaceModel.Nodes.Count(),
                });
                
            }
        }

        private WorkspaceModel WorkspaceFromJSON(string file)
        {
            string json = File.ReadAllText(file);
            //this amazing little portion constructs a DYN from JSON.
            var wm = WorkspaceModel.FromJson(json, theDAM.DynView.Model.LibraryServices,
                null,
                null,
                theDAM.DynView.Model.NodeFactory,
                true,
                true,
                theDAM.DynView.Model.CustomNodeManager);

            return wm;
        }

        //private string GraphPurpose(List<NodeModel> nodes)
        //{

        //}

        private Dictionary<string, string> LoadCategorizationGraphs()
        {
            _categoryDictionary.Clear();
            string extraPath = theDAM._executingPath.Replace("bin\\theDAM.dll", "extra\\Categorization\\");

            foreach (var dyn in Directory.GetFiles(extraPath))
            {
                var ws = WorkspaceFromJSON(dyn);
                foreach (NodeModel node in ws.Nodes)
                {
                    try
                    {
                        _categoryDictionary.Add(node.Name, ws.FileName);
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                    
                }
               
            }
            
            return _categoryDictionary;
        }
    }
}

﻿namespace SQLCompare.Core.Entities.Project
{
    public class CompareProject
    {
        public DatabaseProviderOptions Source { get; set; }

        public DatabaseProviderOptions Target { get; set; }

        public ProjectOptions Options { get; set; }

        // private BaseDb _sourceDB;

        // private BaseDb _targetDB;

        // public int _Compareresult { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using PLR.AST.Actions;
using PLR.AST.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Processes {

    public class ActionPrefix : Process{

        public ActionPrefix(Action action, Process proc) {
            _action = action;
            _proc = proc;
            _children.Add(Action);
            _children.Add(proc);
        }

        private Action _action;
        public Action Action
        {
            get { return _action; }
            set { _action = value; _children[0] = _action; }
        }

        private Process _proc;
        public Process Process {
            get { return _proc; }
            set { _proc = value; _children[1] = _proc; }
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override void Compile(CompileInfo info) {
            Type listType = typeof(List<IAction>);
            Type procType = typeof(ProcessBase);
            ILGenerator il = info.ILGenerator;
            ThisPointer thisP = new ThisPointer(procType);
            //Init and store new List in a local var
            LocalBuilder localList = il.DeclareLocal(listType);
            Assign(localList, New(listType), il);

            NewObject newAction = New(typeof(ChannelSync), _action.Name, thisP, _action is InAction);
            Call(localList, "Add", true, newAction).Compile(il);

            EmitDebug("Preparing to sync now...",il);

            ////Call "Sync" with the list and get the return value back
            //LocalBuilder localChosen = il.DeclareLocal(typeof(int));
            //Assign(localChosen, Call(thisP, "Sync", false, localList), il);
            Call(thisP, "Sync", true, localList).Compile(il);
            this.Process.Compile(info);
        }
    }
}


﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using PLR.AST;
using PLR.AST.Expressions;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.AST.Processes
{
    public class ProcessConstant : Process {
        public ProcessConstant(string name) {
            _name = name;
            _subscript = new Subscript();
            _children.Add(Subscript);
        }

        private string _name;
        public string Name { get { return _name; } }

        protected Subscript _subscript;
        public Subscript Subscript { get { return _subscript; } }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is ProcessConstant))
            {
                return false;
            }
            ProcessConstant other = (ProcessConstant)obj;
            return other.Name == this.Name && other.Subscript.Count == this.Subscript.Count;
        }

        public override int GetHashCode() {
            return (this.Name + this.Subscript.Count).GetHashCode();

        }

        public override void Compile(CompileContext context) {

            foreach (ArithmeticExpression exp in this.Subscript) {
                exp.Compile(context);
            }
            EmitRunProcess(context, context.NamedProcessConstructors[this.Name], false, LexicalInfo);
        }
    }
}

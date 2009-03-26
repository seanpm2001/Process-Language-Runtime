﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {

    public class UnaryMinus : ArithmeticExpression{
        protected ArithmeticExpression _exp;
        public ArithmeticExpression Expression { get { return _exp; } }

        public UnaryMinus(ArithmeticExpression exp) {
            _exp = exp;
            _children.Add(exp);
        }
        public override int Value {
            get { return -this.Expression.Value; }
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        
        public override string ToString() {
            return "-" + _exp.ToString();
        }

        public override void Compile(CompileContext context) {
            _exp.Compile(context);
            context.ILGenerator.Emit(OpCodes.Neg);
        }
    }
}

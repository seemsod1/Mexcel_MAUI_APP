using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Backend;

    class CountingVisitor : CountingBaseVisitor<double>
    {
        
        public override double VisitCompileUnit(CountingParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }
        public override double VisitNumberExpr(CountingParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);
            return result;
        }
        
        public override double VisitIdentifierExpr(CountingParser.IdentifierExprContext context)
        {
            var result = context.GetText();

            //видобути значення змінної з таблиці
            if (Table.IsCyclicDependency(Calculator.EvaluatingCellName,result))
            {
                throw new Exception("Invalid input");
            }
            var evCell = Calculator.CellTable.Cells[Calculator.EvaluatingCellName];
            var cell = Calculator.CellTable.Cells[result];
            if (!evCell.Depends_on.Contains(result))
            {
                evCell.Depends_on.Add(result);
            }
            if (!cell.ObservedBy.Contains(Calculator.EvaluatingCellName))
            {
                cell.ObservedBy.Add(Calculator.EvaluatingCellName);
            }
            return cell.Value;
        }
        public override double VisitParenthesizedExpr(CountingParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }
        public override double VisitExponentiationExpr(CountingParser.ExponentiationExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            Debug.WriteLine("{0}^{1}", left, right);
            return Math.Pow(left, right);
        }
        public override double VisitUnaryExpr(CountingParser.UnaryExprContext context)
        {
            var operand = Visit(context.expression(1));
            if (context.operatorToken.Type == CountingLexer.UNARY_PLUS)
            {
                Debug.WriteLine("+{0}", operand);
                return operand;
            }
            else // CountingLexer.UNARY_MINUS
            {
                Debug.WriteLine("-{0}", operand);
                return -operand;
            }
        }

        public override double VisitComparisonExpr(CountingParser.ComparisonExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            int operatorTokenType = context.operatorToken.Type;

            switch (operatorTokenType)
            {
                case CountingLexer.LESS:
                    Debug.WriteLine("{0} < {1}", left, right);
                    return left < right ? 1.0 : 0.0;
                case CountingLexer.LESS_EQUAL:
                    Debug.WriteLine("{0} <= {1}", left, right);
                    return left <= right ? 1.0 : 0.0;
                case CountingLexer.GREATER:
                    Debug.WriteLine("{0} > {1}", left, right);
                    return left > right ? 1.0 : 0.0;
                case CountingLexer.GREATER_EQUAL:
                    Debug.WriteLine("{0} >= {1}", left, right);
                    return left >= right ? 1.0 : 0.0;
                case CountingLexer.NOT_EQUAL:
                    Debug.WriteLine("{0} != {1}", left, right);
                    return left != right ? 1.0 : 0.0;
                default:
                    return 0.0; // Handle other cases or errors as needed
            }
        }

        public override double VisitAdditiveExpr(CountingParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == CountingLexer.ADD)
            {
                Debug.WriteLine("{0}+{1}", left, right);
                return left + right;
            }
            else //LabCalculatorLexer.SUBTRACT
            {
                Debug.WriteLine("{0}-{1}", left, right);
                return left - right;
            }
        }
        public override double VisitMultiplicativeExpr(CountingParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == CountingLexer.MULTIPLY)
            {
                Debug.WriteLine("{0}*{1}", left, right);
                return left * right;
            }
            else //LabCalculatorLexer.DIVIDE
            {
                Debug.WriteLine("{0}/{1}", left, right);
                return left / right;
            }
        }
        private double WalkLeft(CountingParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<CountingParser.ExpressionContext>(0));
        }
        private double WalkRight(CountingParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<CountingParser.ExpressionContext>(1));
        }
    }


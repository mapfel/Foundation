﻿using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq.Expressions;

namespace Foundation.Linq.Expressions;

[TestFixture]
public class ExpressionExtensionsTests
{
    [Test]
    public void GetExpressionHashCode_Should_ReturnDifferentHashCodes_When_UsingBinaryExpressionIsEqual_DifferentParameterTypes()
    {
        var hashCode1 = Scope.Returns(() =>
        {
            var left = Expression.Parameter(typeof(string), "x");
            var right = Expression.Constant("5");

            var expression = Expression.MakeBinary(ExpressionType.Equal, left, right);
            return expression.GetExpressionHashCode();
        });

        var hashCode2 = Scope.Returns(() =>
        {
            var left = Expression.Parameter(typeof(int), "x");
            var right = Expression.Constant(5);

            var expression = Expression.MakeBinary(ExpressionType.Equal, right, left);
            return expression.GetExpressionHashCode();
        });

        hashCode1.Should().NotBe(hashCode2);
    }

    [Test]
    public void GetExpressionHashCode_Should_ReturnSameHashCodes_When_UsingBinaryExpressionIsEqual_SameParameterTypes()
    {
        var left = Expression.Parameter(typeof(string), "x");
        var right = Expression.Constant("Test");

        var hashCode1 = Scope.Returns(() =>
        {
            var expression = Expression.MakeBinary(ExpressionType.Equal, left, right);
            return expression.GetExpressionHashCode();
        });

        var hashCode2 = Scope.Returns(() =>
        {
            var expression = Expression.MakeBinary(ExpressionType.Equal, right, left);
            return expression.GetExpressionHashCode();
        });

        hashCode1.Should().Be(hashCode2);
    }

    [Test]
    public void GetExpressionHashCode_Should_ReturnSameHashCodes_When_UsingLambdaExpression_BodyIsConstant_DifferentParameterNames()
    {
        Expression<Func<string, bool>> expression1 = x => true;
        var hashCodeWithX = expression1.GetExpressionHashCode();

        Expression<Func<string, bool>> expression2 = a => true;
        var hashCodeWithA = expression2.GetExpressionHashCode();

        hashCodeWithX.Should().Be(hashCodeWithA);
    }

    [Test]
    public void GetExpressionHashCode_Should_ReturnSameHashCodes_When_LambdaExpression_BodyNotEqual()
    {
        Expression<Func<int, bool>> expression1 = x => x != 12;
        var hashCodeWithX = expression1.GetExpressionHashCode();

        Expression<Func<int, bool>> expression2 = a => 12 != a;
        var hashCodeWithA = expression2.GetExpressionHashCode();

        hashCodeWithX.Should().Be(hashCodeWithA);
    }

    [Test]
    public void GetExpressionHashCode_Should_ReturnSameHashCodes_When_LambdaExpression_BodyOr()
    {
        Expression<Func<int, bool>> expression1 = x => x == 5 || x == 7;
        var hashCodeWithX = expression1.GetExpressionHashCode();

        Expression<Func<int, bool>> expression2 = a => 7 == a || a == 5;
        var hashCodeWithA = expression2.GetExpressionHashCode();

        hashCodeWithX.Should().Be(hashCodeWithA);
    }

    [Test]
    public void GetExpressionHashCode_Should_ReturnSameHashCodes_When_LambdaExpression_BodyOr_DifferentBinaryExpressions()
    {
        Expression<Func<int, bool>> expression1 = x => x == 5 || x == 7;
        var hashCodeWithX = expression1.GetExpressionHashCode();

        Expression<Func<int, bool>> expression2 = a => 7 == a || a != 5;
        var hashCodeWithA = expression2.GetExpressionHashCode();

        hashCodeWithX.Should().NotBe(hashCodeWithA);
    }

    [Test]
    public void GetExpressionHashCode_Should_ReturnSameHashCodes_When_UsingParameterExpression_DifferentNames_With_ConsiderNamesFalse()
    {
        var hashCode1 = Expression.Parameter(typeof(string), "x").GetExpressionHashCode(false);
        var hashCode2 = Expression.Parameter(typeof(string), "a").GetExpressionHashCode(false);

        hashCode1.Should().Be(hashCode2);
    }
}
﻿using Thor.Analyzer.Rules;
using Thor.Analyzer.Tests.EventSources;
using FluentAssertions;
using Moq;
using System.Linq;
using Xunit;

namespace Thor.Analyzer.Tests.Rules
{
    public class EventParametersMustBeInOrderTests
        : EventRuleTestBase<EventParametersMustBeInOrder>
    {
        protected override EventParametersMustBeInOrder CreateRule(IRuleSet ruleSet)
        {
            return new EventParametersMustBeInOrder(ruleSet);
        }

        [Fact(DisplayName = "Apply: Should return an error if event parameters were not in order")]
        public void Apply_Error()
        {
            // arrange
            WrongEventParameterOrderEventSource eventSource =
                WrongEventParameterOrderEventSource.Log;
            SchemaReader reader = new SchemaReader(eventSource);
            EventSourceSchema schema = reader.Read();
            IRuleSet ruleSet = new Mock<IRuleSet>().Object;
            IEventRule rule = CreateRule(ruleSet);
        
            // act
            IResult result = rule.Apply(schema.Events.First(), eventSource);
        
            // assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Error>();
        }

        [Fact(DisplayName = "Apply: Should return a success if event parameters were in order")]
        public void Apply_Success()
        {
            // arrange
            CorrectEventParameterOrderEventSource eventSource =
                CorrectEventParameterOrderEventSource.Log;
            SchemaReader reader = new SchemaReader(eventSource);
            EventSourceSchema schema = reader.Read();
            IRuleSet ruleSet = new Mock<IRuleSet>().Object;
            IEventRule rule = CreateRule(ruleSet);

            // act
            IResult result = rule.Apply(schema.Events.First(), eventSource);

            // assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Success>();
        }

        [Fact(DisplayName = "Apply: Should return a success if event parameters were in order (related activity id)")]
        public void Apply_SuccessRelatedActivityId()
        {
            // arrange
            CorrectEventParameterOrderWithRelatedActivityIdEventSource eventSource =
                CorrectEventParameterOrderWithRelatedActivityIdEventSource.Log;
            SchemaReader reader = new SchemaReader(eventSource);
            EventSourceSchema schema = reader.Read();
            IRuleSet ruleSet = new Mock<IRuleSet>().Object;
            IEventRule rule = CreateRule(ruleSet);

            // act
            IResult result = rule.Apply(schema.Events.First(), eventSource);

            // assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Success>();
        }
    }
}
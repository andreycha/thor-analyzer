﻿using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;

namespace ChilliCream.Tracing.Analyzer.Rules
{
    /// <summary>
    /// A rule which checks if the <see cref="EventSource"/> matches the schema.
    /// </summary>
    public class EventMustBeInvokable
        : IEventRule
    {
        /// <summary>
        /// Initiates a new instance of the <see cref="EventMustBeInvokable"/> class.
        /// </summary>
        /// <param name="ruleSet">A ruleset which is the parent of this rule.</param>
        public EventMustBeInvokable(IRuleSet ruleSet)
        {
            if (ruleSet == null)
            {
                throw new ArgumentNullException(nameof(ruleSet));
            }

            RuleSet = ruleSet;
        }

        /// <inheritdoc/>
        public IRuleSet RuleSet { get; }

        /// <inheritdoc/>
        public IResult Apply(EventSchema schema, EventSource eventSource)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            Type eventSourceType = eventSource.GetType();

            using (ProbeEventListener listener = new ProbeEventListener())
            {
                try
                {
                    listener.EnableEvents(eventSource, schema.Level, schema.Keywords);

                    MethodInfo method = eventSource.GetMethodFromSchema(schema);
                    object[] values = method.GetParameters()
                        .Select(p => p.ParameterType.NotDefault())
                        .ToArray();
                    string exceptionMessage = null;

                    if (method == null ||
                        !eventSource.TryInvokeMethod(schema, method, values, out exceptionMessage) ||
                        listener.OrderdEvents.Count() == 0)
                    {
                        string[] details = (string.IsNullOrWhiteSpace(exceptionMessage))
                            ? new string[0]
                            : new[] { exceptionMessage };

                        return new Error(this, $"Event method '{method.Name}' does not call WriteEvent() or " +
                            "the call is bypassed due to incorrect filtering before the WriteEvent call.", details);
                    }
                }
                finally
                {
                    listener.DisableEvents(eventSource);
                }
            }

            return new Success(this);
        }
    }
}
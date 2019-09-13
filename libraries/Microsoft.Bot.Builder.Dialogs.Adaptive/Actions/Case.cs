﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class Case
    {
        public Case(string value = null, IEnumerable<Dialog> actions = null)
        {
            this.Value = value;
            this.Actions = actions?.ToList() ?? this.Actions;
        }

        /// <summary>
        /// Gets or sets value expression to be compared against condition.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets set of actions to be executed given that the condition of the switch matches the value of this case.
        /// </summary>
        [JsonProperty("actions")]
        public List<Dialog> Actions { get; set; } = new List<Dialog>();

        /// <summary>
        /// Creates an expression that returns the value in its primitive type. Still
        /// assumes that switch case values are compile time constants and not expressions
        /// that can be evaluated against state.
        /// </summary>
        /// <returns>An expression that reflects the constant case value.</returns>
        public Expression CreateValueExpression()
        {
            Expression expression = null;

            if (long.TryParse(Value, out long i))
            {
                expression = Expression.ConstantExpression(i);
            }
            else if (float.TryParse(Value, out float f))
            {
                expression = Expression.ConstantExpression(f);
            }
            else if (bool.TryParse(Value, out bool b))
            {
                expression = Expression.ConstantExpression(b);
            }
            else
            {
                expression = Expression.ConstantExpression(Value);
            }

            return expression;
        }
    }
}

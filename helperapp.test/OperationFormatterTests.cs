using System;
using System.Collections.Generic;
using Xunit;

namespace helperapp.test
{
    public class OperationFormatterTests
    {
        [Fact]
        public void PlainStringIsUnchanged()
        {
            var properties = new Dictionary<string, string>();
            var input = @"This\is\an(input)\string and it: will remain %unchanged%";
            var output = OperationFormatter.Format(input, properties);

            var target = input;
            Assert.Equal(target, output);
        }

        [Fact]
        public void EnvironmentVariableIsExpanded()
        {
            var properties = new Dictionary<string, string>();
            var input = @"%temp% will expand only when it's in braces, like (%temp%).";
            var output = OperationFormatter.Format(input, properties);

            var target = $@"%temp% will expand only when it's in braces, like {Environment.ExpandEnvironmentVariables("%temp%")}.";
            Assert.Equal(target, output);
        }

        [Fact]
        public void KnownPropertyIsExpanded()
        {
            var properties = new Dictionary<string, string>() { { "sample", "I'm expanded!" } };
            var input = @"sample will expand only when it's in braces, like (sample).";
            var output = OperationFormatter.Format(input, properties);

            var target = @"sample will expand only when it's in braces, like I'm expanded!.";
            Assert.Equal(target, output);
        }

        [Fact]
        public void UnknownPropertyIsSkipped()
        {
            var properties = new Dictionary<string, string>() { { "sample", "I'm expanded!" } };
            var input = @"sample will expand only when it's known, (not like this).";
            var output = OperationFormatter.Format(input, properties);

            var target = input;
            Assert.Equal(target, output);
        }

        [Fact]
        public void MultiplePropertiesAreExpanded()
        {
            var properties = new Dictionary<string, string>() { { "sample", "I'm expanded!" }, { "bar", "baz"} };
            var input = @"Here we have (%temp%), (sample), (foo) and (bar).";
            var output = OperationFormatter.Format(input, properties);

            var target = $@"Here we have {Environment.ExpandEnvironmentVariables("%temp%")}, I'm expanded!, (foo) and baz.";
            Assert.Equal(target, output);
        }
    }
}

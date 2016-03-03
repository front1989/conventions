using System.Threading.Tasks;
using GraphQL.Conventions.Adapters.Engine;
using GraphQL.Conventions.Attributes.MetaData.Relay;
using GraphQL.Conventions.Tests.Templates;
using GraphQL.Conventions.Tests.Templates.Extensions;
using GraphQL.Conventions.Types;
using Xunit;

namespace GraphQL.Conventions.Tests.Attributes.MetaData.Relay
{
    public class ImplementViewerAttributeTests : TestBase
    {
        [Fact]
        public void Can_Generate_A_Viewer_Node_For_A_Schema()
        {
            GetSchemaDefinition(false).ShouldEqualWhenReformatted(@"
            type Query1 {
                intToString(value: Int!): String
                viewer: QueryViewer
            }
            type QueryViewer {
                intToString(value: Int!): String
            }
            ");
        }

        [Fact]
        public async void Can_Use_The_Viewer_Node_For_A_Schema()
        {
            var result = await ExecuteQuery(false, @"
            {
                intToString(value: 5)
                viewer {
                    intToString(value: 5)
                }
            }");
            result.ShouldHaveNoErrors();
            result.Data.ShouldHaveFieldWithValue("intToString", "5");
            result.Data.ShouldHaveFieldWithValue("viewer", "intToString", "5");
        }

        [Fact]
        public void Can_Generate_A_Viewer_Node_For_Multiple_Schemas()
        {
            GetSchemaDefinition(true).ShouldEqualWhenReformatted(@"
            type Query {
                floatToString(value: Float!): String
                intToString(value: Int!): String
                viewer: QueryViewer
            }
            type QueryViewer {
                floatToString(value: Float!): String
                intToString(value: Int!): String
            }
            ");
        }

        [Fact]
        public async void Can_Use_The_Viewer_Node_For_Multiple_Schemas()
        {
            var result = await ExecuteQuery(true, @"
            {
                floatToString(value: 3.14)
                intToString(value: 5)
                viewer {
                    floatToString(value: 3.14)
                    intToString(value: 5)
                }
            }");
            result.ShouldHaveNoErrors();
            result.Data.ShouldHaveFieldWithValue("intToString", "5");
            result.Data.ShouldHaveFieldWithValue("floatToString", "3.14");
            result.Data.ShouldHaveFieldWithValue("viewer", "intToString", "5");
            result.Data.ShouldHaveFieldWithValue("viewer", "floatToString", "3.14");
        }

        [Fact]
        public void Can_Generate_Viewers_For_Multiple_Operations()
        {
            GetSchemaDefinition(true, true).ShouldEqualWhenReformatted(@"
            type Mutation {
                doSomething(value: Boolean): Boolean
                doSomethingElse(value: Boolean): Boolean
                viewer: MutationViewer
            }
            type MutationViewer {
                doSomething(value: Boolean): Boolean
                doSomethingElse(value: Boolean): Boolean
            }
            type Query {
                floatToString(value: Float!): String
                intToString(value: Int!): String
                viewer: QueryViewer
            }
            type QueryViewer {
                floatToString(value: Float!): String
                intToString(value: Int!): String
            }
            ");
        }

        private string GetSchemaDefinition(bool useMultiple, bool includeMutations = false)
        {
            if (includeMutations)
            {
                var engine = new GraphQLEngine(
                    typeof(SchemaDefinition<Query1, Mutation1>),
                    typeof(SchemaDefinition<Query2, Mutation2>));
                return engine.Describe();
            }
            else
            {
                var engine = useMultiple
                    ? new GraphQLEngine(typeof(SchemaDefinition<Query1>), typeof(SchemaDefinition<Query2>))
                    : new GraphQLEngine(typeof(SchemaDefinition<Query1>));
                return engine.Describe();
            }
        }

        private async Task<ExecutionResult> ExecuteQuery(bool useMultiple, string query)
        {
            var engine = useMultiple
                ? new GraphQLEngine(typeof(SchemaDefinition<Query1>), typeof(SchemaDefinition<Query2>))
                : new GraphQLEngine(typeof(SchemaDefinition<Query1>));
            var result = await engine
                .NewExecutor()
                .WithQueryString(query)
                .Execute();
            return result;
        }

        [ImplementViewer(OperationType.Query)]
        class Query1
        {
            public string IntToString(int value) => value.ToString();
        }

        [ImplementViewer(OperationType.Query)]
        class Query2
        {
            public string FloatToString(float value) => value.ToString();
        }

        [ImplementViewer(OperationType.Mutation)]
        class Mutation1
        {
            public bool? DoSomething(bool? value) => value;
        }

        [ImplementViewer(OperationType.Mutation)]
        class Mutation2
        {
            public bool? DoSomethingElse(bool? value) => value;
        }
    }
}

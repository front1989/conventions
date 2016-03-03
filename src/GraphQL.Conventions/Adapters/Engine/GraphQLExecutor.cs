using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Conventions.Adapters.Engine.Utilities;
using GraphQL.Conventions.Profiling;
using GraphQL.Conventions.Types.Resolution;
using GraphQL.Validation;

namespace GraphQL.Conventions.Adapters.Engine
{
    public class GraphQLExecutor : IGraphQLExecutor<ExecutionResult>
    {
        private readonly GraphQLEngine _engine;

        private readonly IRequestDeserializer _requestDeserializer;

        private object _rootObject;

        private object _userContext;

        private string _queryString;

        private string _operationName;

        private Inputs _inputs;

        private CancellationToken _cancellationToken = default(CancellationToken);

        private bool _useValidation = true;

        internal GraphQLExecutor(GraphQLEngine engine, IRequestDeserializer requestDeserializer)
        {
            _engine = engine;
            _requestDeserializer = requestDeserializer;
        }

        public IGraphQLExecutor<ExecutionResult> WithRequest(string requestBody)
        {
            var query = _requestDeserializer.GetQueryFromRequestBody(requestBody);
            _queryString = query.QueryString;
            _operationName = query.OperationName;
            return this.WithInputs(query.Variables);
        }

        public IGraphQLExecutor<ExecutionResult> WithQueryString(string queryString)
        {
            _queryString = queryString;
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> WithOperationName(string operationName)
        {
            _operationName = operationName;
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> WithInputs(Dictionary<string, object> inputs)
        {
            _inputs = inputs != null ? new Inputs(inputs) : new Inputs();
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> WithRootObject(object rootValue)
        {
            _rootObject = rootValue;
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> WithUserContext(object userContext)
        {
            _userContext = userContext;
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> WithCancellationToken(CancellationToken token)
        {
            _cancellationToken = token;
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> WithDependencyInjector(IDependencyInjector injector)
        {
            _engine.DependencyInjector = injector;
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> WithProfiler(IProfiler profiler)
        {
            _engine.AddProfiler(profiler);
            return this;
        }

        public IGraphQLExecutor<ExecutionResult> UseValidation(bool useValidation = true)
        {
            _useValidation = useValidation;
            return this;
        }

        public async Task<ExecutionResult> Execute() =>
            await _engine
                .Execute(_rootObject, _queryString, _operationName, _inputs, _userContext, _useValidation, null, _cancellationToken)
                .ConfigureAwait(false);

        public async Task<string> ExecuteAndSerializeResponse() => _engine.ConvertResultToString(await Execute());

        public IValidationResult Validate() => _engine.Validate(_queryString);
    }
}

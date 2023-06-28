using System.Threading.Tasks;
using Domain.Types.Entities.Generated;
using Domain.Types.Types.Ids;
using FrontEnd.Api.Client;
using Frontend.Server.Api.IntergrationTests.NoneTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frontend.Server.Api.IntergrationTests.Tests.KeywordsUpdateFieldsApi
{
    [TestClass]
    public sealed class KeywordSetIsForcedTests : ApiTestsBase
    {
        [TestMethod]
        public void ShouldBeDone()
        {
            NewTestFlow()
                .CreateUserWithApiClient(out var client)
                .CreateSite()
                .CreateKeyword(out var keyword)
                .Assert(keyword.IsForced).ToBe(Keyword.Defaults.IS_FORCED)
                
                .Action(() => client.KeywordSetIsForceAsync(keyword.KeywordId, true))
                .ReloadKeyword(out keyword)
                .Assert(keyword.IsForced).ToBe(true)
                .Assert(keyword.DateUpdated).ToBeAroundNow()
                
                .Action(() => client.KeywordSetIsForceAsync(keyword.KeywordId, false))
                .ReloadKeyword(out keyword)
                .Assert(keyword.IsForced).ToBe(false)
                .Assert(keyword.DateUpdated).ToBeAroundNow()
            ;
        }
        
        [TestMethod] public void ShouldHasRestrictedAccess() => ShouldHasRestrictedAccess(ApiMethodCall);
        [TestMethod] public void ShouldRequireAuth() => ShouldRequireAuth(ApiMethodCall);
        [TestMethod] public void ShouldReturnNotFound() => ShouldReturnNotFound(ApiMethodCall);

        private Task ApiMethodCall(FrontApiClient client, KeywordId keywordId)
        {
            return client.KeywordSetIsForceAsync(keywordId, true);
        }
    }
}

using System.Collections.Generic;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class ConditionalPatchTests : IClassFixture<ProductTestFixture>
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public ConditionalPatchTests(ProductTestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        product = testFixture.Product;
        productKey = testFixture.Key;
    }

    [Fact]
    public async void PatchWithReferenceSucceeds()
    {
        var existingItem = await collection.GetAsync<Product>(productKey);

        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/description", Value = "Updated Description" });

        var kvMetaData = await collection.PatchAsync(productKey, patchOperations.ToArray(), existingItem.VersionReference);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<Product>(productKey);
        var product = kvObject.Value;
        Assert.Equal("Updated Description", product.Description);
    }

    [Fact]
    public async void ThrowsNotFoundExceptionWhenPassingInvalidKey()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.PatchAsync("2", patchOperations.ToArray(), kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.PatchAsync("1", patchOperations.ToArray(), "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}

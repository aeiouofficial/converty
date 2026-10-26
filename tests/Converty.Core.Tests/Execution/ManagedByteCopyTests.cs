using System.Reflection;
using System.Security.Cryptography;
using Converty.Core.Execution;

namespace Converty.Core.Tests.Execution;

public sealed class ManagedByteCopyTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "converty-managed-copy-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task CopyProducesByteExactOutputAndReturnsMatchingSha256()
    {
        Directory.CreateDirectory(_root);
        string input = Path.Combine(_root, "source.mp4");
        string output = Path.Combine(_root, "source.partial.mp4");
        byte[] payload = [0x00, 0x01, 0x02, 0x7f, 0x80, 0xff, 0x42];
        File.WriteAllBytes(input, payload);

        string digest = await InvokeCopyAsync(input, output);

        Assert.Equal(payload, File.ReadAllBytes(output));
        Assert.Equal(Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant(), digest);
    }

    [Fact]
    public async Task CopyNeverOverwritesExistingStagedOutput()
    {
        Directory.CreateDirectory(_root);
        string input = Path.Combine(_root, "source.webm");
        string output = Path.Combine(_root, "source.partial.webm");
        File.WriteAllBytes(input, [1, 2, 3]);
        File.WriteAllBytes(output, [9, 9, 9]);

        await Assert.ThrowsAsync<IOException>(() => InvokeCopyAsync(input, output));

        Assert.Equal([9, 9, 9], File.ReadAllBytes(output));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static async Task<string> InvokeCopyAsync(string inputPath, string outputPath)
    {
        Type? copyType = typeof(IConversionWorkerClient).Assembly.GetType(
            "Converty.Core.Execution.ManagedByteCopy",
            throwOnError: false,
            ignoreCase: false);
        Assert.NotNull(copyType);

        MethodInfo? copyMethod = copyType!.GetMethod(
            "CopyAndVerifyAsync",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(string), typeof(string), typeof(CancellationToken)],
            modifiers: null);
        Assert.NotNull(copyMethod);

        object? invocation = copyMethod!.Invoke(
            null,
            [inputPath, outputPath, TestContext.Current.CancellationToken]);
        Task<string> task = Assert.IsAssignableFrom<Task<string>>(invocation);
        return await task;
    }
}

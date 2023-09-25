// Copyright (c) Microsoft. All rights reserved.

namespace PrepareDocs;

internal record class AppOptions(
    string Files,
    string? Category,
    bool SkipBlobs,
    string? StorageServiceBlobConnectionString,
    string? Container,
    string? SearchServiceEndpoint,
    string? SearchServiceKey,
    string? SearchIndexName,
    bool Remove,
    bool RemoveAll,
    string? FormRecognizerServiceEndpoint,
    string? FormRecognizerServiceKey,
    bool Verbose,
    IConsole Console) : AppConsole(Console);

internal record class AppConsole(IConsole Console);

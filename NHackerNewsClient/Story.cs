namespace NHackerNewsClient;

public sealed record Story(
    string? Title,
    string? Uri,
    string? PostedBy,
    int Time,
    int Score,
    int CommentCount
);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var pullRequests = new List<PullRequest>();
var nextId = 1;

app.MapPost("/api/pullrequests", (CreatePullRequestDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest("Title cannot be empty");

    var pr = new PullRequest
    {
        Id = nextId++,
        Title = dto.Title,
        AuthorId = dto.AuthorId,
        TeamMembers = dto.TeamMembers
    };
    pullRequests.Add(pr);
    return Results.Ok(pr);
});

app.MapPost("/api/pullrequests/{id}/assign", (int id) =>
{
    var pr = pullRequests.FirstOrDefault(p => p.Id == id);
    if (pr == null) return Results.NotFound("PR not found");

    var available = pr.TeamMembers.Where(m => m != pr.AuthorId).ToList();
    if (available.Count == 0) return Results.BadRequest("No available reviewers");

    var random = new Random();
    pr.AssignedReviewers = available.OrderBy(x => random.Next()).Take(2).ToList();
    return Results.Ok(pr);
});

app.MapGet("/api/users/{userId}/pullrequests", (int userId) =>
{
    var myPrs = pullRequests.Where(p => p.AuthorId == userId).ToList();
    return Results.Ok(myPrs);
});

app.MapGet("/api/ml/export", () =>
{
    var dataForMl = pullRequests.Where(p => p.AssignedReviewers.Count > 0).ToList();
    return Results.Ok(dataForMl);
});

app.Run();

public class PullRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public List<int> TeamMembers { get; set; } = new();
    public List<int> AssignedReviewers { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Open";
}

public class CreatePullRequestDto
{
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public List<int> TeamMembers { get; set; } = new();
}
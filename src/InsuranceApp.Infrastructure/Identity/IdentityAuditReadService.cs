using System.Text;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Audit;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Identity;

public class IdentityAuditReadService(InsuranceDbContext dbContext) : IIdentityAuditReadService
{
    public async Task<IdentityAuditLogQueryResponse> QueryAsync(IdentityAuditLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : Math.Min(request.PageSize, 200);

        var query = BuildFilteredQuery(request);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new IdentityAuditLogDto
            {
                Id = x.Id,
                Action = x.Action,
                Outcome = x.Outcome,
                SubjectId = x.SubjectId,
                Description = x.Description,
                CorrelationId = x.CorrelationId,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new IdentityAuditLogQueryResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<string> ExportCsvAsync(IdentityAuditLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        var rows = await BuildFilteredQuery(request)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new IdentityAuditLogDto
            {
                Id = x.Id,
                Action = x.Action,
                Outcome = x.Outcome,
                SubjectId = x.SubjectId,
                Description = x.Description,
                CorrelationId = x.CorrelationId,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("Id,CreatedAtUtc,Action,Outcome,SubjectId,Description,CorrelationId,IpAddress,UserAgent");

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',',
                row.Id,
                Escape(row.CreatedAtUtc.ToString("O")),
                Escape(row.Action),
                Escape(row.Outcome),
                Escape(row.SubjectId),
                Escape(row.Description),
                Escape(row.CorrelationId),
                Escape(row.IpAddress),
                Escape(row.UserAgent)));
        }

        return builder.ToString();
    }

    private IQueryable<Domain.Entities.IdentityAuditLog> BuildFilteredQuery(IdentityAuditLogQueryRequest request)
    {
        var query = dbContext.IdentityAuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action == request.Action);
        }

        if (!string.IsNullOrWhiteSpace(request.Outcome))
        {
            query = query.Where(x => x.Outcome == request.Outcome);
        }

        if (!string.IsNullOrWhiteSpace(request.SubjectId))
        {
            query = query.Where(x => x.SubjectId == request.SubjectId);
        }

        if (request.FromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= request.ToUtc.Value);
        }

        return query;
    }

    private static string Escape(string value)
    {
        var normalized = value.Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }
}

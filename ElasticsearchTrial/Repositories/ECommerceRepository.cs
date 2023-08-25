﻿using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticsearchTrial.Constants;
using ElasticsearchTrial.Entities.ECommerceEntities;
using System.Collections.Immutable;

namespace ElasticsearchTrial.Repositories;

public class ECommerceRepository
{
    private readonly ElasticsearchClient _client;

    public ECommerceRepository(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<ImmutableList<ECommerce>> TermQueryAsync(string customerFirstName)
    {
        //1. way
        //var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName).Query(q => q.Term(t => t.Field("customer_first_name.keyword").Value(customerFirstName))));

        //2. way
        //var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName).Query(q => q.Term(t => t.CustomerFirstName.Suffix("keyword"), customerFirstName)));

        //3. way

        var termQuery = new TermQuery("customer_first_name.keyword") { Value = customerFirstName, CaseInsensitive = true };

        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName).Query(termQuery));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> TermsQueryAsync(List<string> customerFirstNameList)
    {
        List<FieldValue> terms = new List<FieldValue>();
        customerFirstNameList.ForEach(x => terms.Add(x));

        //1.way
        //var termsQuery = new TermsQuery()
        //{
        //    Field = "customer_first_name.keyword",
        //    Terms = new TermsQueryField(terms.AsReadOnly())
        //};

        //var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName).Query(termsQuery));

        //2. way
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Size(100)
            .Query(q => q
            .Terms(t => t
            .Field(f => f.CustomerFirstName
            .Suffix("keyword"))
            .Terms(new TermsQueryField(terms.AsReadOnly())))));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> PrefixQueryAsync(string customerFullName)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
                .Query(q => q
                    .Prefix(p => p
                        .Field(f => f.CustomerFullName
                            .Suffix("keyword"))
                                .Value(customerFullName))));
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> RangeQueryAsync(double fromPrice, double toPrice)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
                .Query(q => q
                    .Range(p => p
                        .NumberRange(nr => nr
                            .Field(f => f.TaxfulTotalPrice)
                                .Gte(fromPrice).Lte(toPrice)))));
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> MatchAllQueryAsync()
    {
        //var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
        //    .Size(100)
        //    .Query(q => q.MatchAll()));

        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Size(1000)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.CustomerFullName)
                        .Query("shaw"))));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> PaginationQueryAsync(int page, int pageSize)
    {
        // page=1, pageSize=10 =>  1-10
        // page=2, pageSize=10 => 11-20
        // page=3, pageSize=10 => 21-30

        var pageFrom = (page - 1) * pageSize;

        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Size(pageSize).From(pageFrom)
            .Query(q => q.MatchAll()));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> WildCardQueryAsync(string customerFullName)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Query(q => q
            .Wildcard(w => w
                .Field(f => f.CustomerFullName
                    .Suffix("keyword"))
                        .Wildcard(customerFullName))));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> FuzzyQueryAsync(string customerName)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Query(q => q
                .Fuzzy(fu => fu
                    .Field(f => f.CustomerFirstName
                        .Suffix("keyword"))
                            .Value(customerName)
                                .Fuzziness(new Fuzziness(2))))
                .Sort(sort => sort
                    .Field(f => f.TaxfulTotalPrice, new FieldSort() { Order = SortOrder.Desc })));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> MatchQueryFullTextAsync(string categoryName)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Size(100)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Category)
                        .Query(categoryName).Operator(Operator.And))));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> MatchBoolPrefixFullTextAsync(string customerFullName)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Size(100)
            .Query(q => q
                .MatchBoolPrefix(m => m
                    .Field(f => f.CustomerFullName)
                        .Query(customerFullName))));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> MatchPhraseFullTextAsync(string customerFullName)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Size(100)
            .Query(q => q
                .MatchPhrase(m => m
                    .Field(f => f.CustomerFullName)
                        .Query(customerFullName))));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> CompoundQueryExampleOneAsync(string cityName, double taxfulTotalPrice, string categoryName, string manufacturer)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s.Index(Messages.ECommerceIndexName)
            .Size(100)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Term(t => t
                            .Field("geoip.city_name")
                            .Value(cityName)))
                    .MustNot(mn => mn
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(f => f.TaxfulTotalPrice)
                                .Lte(taxfulTotalPrice))))
                    .Should(s => s
                        .Term(t => t
                            .Field(f => f.Category.Suffix("keyword"))
                            .Value(categoryName)))
                    .Filter(f => f
                        .Term(t => t
                            .Field("manufacturer.keyword")
                            .Value(manufacturer)))
                    )));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }
}
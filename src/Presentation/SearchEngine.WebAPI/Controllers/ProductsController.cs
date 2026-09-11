using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Products.Commands.CreateProduct;
using SearchEngine.Application.Features.Products.Commands.UpdateProduct;
using SearchEngine.Application.Features.Products.Commands.DeleteProduct;
using SearchEngine.Application.Features.Products.DTOs;
using SearchEngine.Application.Features.Products.Queries.GetAllProducts;
using SearchEngine.Application.Features.Products.Queries.GetProductById;
using SearchEngine.Application.Common.Models;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("api")]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("products", "view")]
    public async Task<IActionResult> GetAll(
        [FromQuery]
        PaginationRequest request)
    {
        var result = await _mediator.Send(
            new GetAllProductsQuery(
                request));

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("products", "view")]
    public async Task<IActionResult> GetById(
        Guid id)
    {
        var result = await _mediator.Send(
            new GetProductByIdQuery(id));

        return Ok(result);
    }

    [HttpPost]
    [HasPermission("products", "create")]
    public async Task<IActionResult> Create(
        CreateProductRequest request)
    {
        var result = await _mediator.Send(
            new CreateProductCommand(request));

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("products", "update")]
    public async Task<IActionResult>
        Update(
            Guid id,
            UpdateProductRequest request)
    {
        var result =
            await _mediator.Send(
                new UpdateProductCommand(
                    id,
                    request));

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("products", "delete")]
    public async Task<IActionResult>
        Delete(
            Guid id)
    {
        var result =
            await _mediator.Send(
                new DeleteProductCommand(
                    id));

        return Ok(result);
    }

}

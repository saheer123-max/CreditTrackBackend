using CreditTrack.Application.DTOs;
using CreditTrack.Domain.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Application.Commands.Products
{
    public record UpdateProductCommand(int ProductId, ProductDto ProductDto) : IRequest<Product>;
}


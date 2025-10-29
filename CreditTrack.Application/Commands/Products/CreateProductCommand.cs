using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using CreditTrack.Application.DTOs;

namespace CreditTrack.Application.Commands.Products
{
    public record CreateProductCommand(ProductDto ProductDto) : IRequest<int>;

}



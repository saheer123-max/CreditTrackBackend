using CreditTrack.Domain.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Application.Queries.Products
{
    public record GetProductByIdQuery(int ProductId) : IRequest<Product>;
}

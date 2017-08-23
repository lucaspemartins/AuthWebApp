using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AuthWebApp.Models;
using AuthWebApp.br.com.correios.ws;
using AuthWebApp.CRMClient;
using System.Globalization;

namespace AuthWebApp.Controllers
{
    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("frete")]
        public IHttpActionResult CalculaFrete(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return Content(HttpStatusCode.NotFound, "Order does not exist.");
            }

            if (User.IsInRole("ADMIN") || User.Identity.Name == order.email)
            {
                CRMRestClient crmClient = new CRMRestClient();
                Customer customer = crmClient.GetCustomerByEmail(order.email);
                if (customer != null)
                {
                    decimal altura = 0, comprimento = 0, largura = 0;
                    order.pesoTotal = 0;
                    order.precoTotal = 0;
                    foreach (OrderItem orderItem in order.OrderItems)
                    {
                        int quantidade = orderItem.quantidade;
                        if (orderItem.Product.altura > altura)
                        {
                            altura = orderItem.Product.altura;
                        }
                        if (orderItem.Product.largura > largura)
                        {
                            largura = orderItem.Product.largura;
                        }
                        comprimento += orderItem.Product.comprimento * quantidade;
                        order.pesoTotal += orderItem.Product.peso * quantidade;
                        order.precoTotal += orderItem.Product.preco * quantidade;
                    }
                    string frete;
                    CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
                    cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", "37540000", customer.zip, order.pesoTotal.ToString(), 1, comprimento, altura, largura, 0, "N", 100, "S");
                    if (resultado.Servicos[0].Erro.Equals("0"))
                    {
                        NumberFormatInfo numberFormat = new NumberFormatInfo();
                        numberFormat.NumberDecimalSeparator = ",";
                        order.precoFrete = Decimal.Parse(resultado.Servicos[0].Valor, numberFormat);
                        order.dataEntrega = order.dataPedido.AddDays(Int32.Parse(resultado.Servicos[0].PrazoEntrega));
                        frete = "Valor do frete: " + order.precoFrete + " - Prazo de entrega: " + order.dataEntrega;
                        db.SaveChanges();

                        return Ok(frete);
                    }
                    else
                    {
                        return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + "-" + resultado.Servicos[0].MsgErro);
                    }
                }
                else
                {
                    return BadRequest("Falha ao consultar o CRM");
                }
            }

            return BadRequest("Authorization Denied! Only admin or the order owner allowed!");
        }

        // GET: api/Orders
        [Authorize(Roles = "ADMIN")]
        public List<Order> GetOrders()
        {
            return db.Orders.ToList();
        }

        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return Content(HttpStatusCode.NotFound, "Order does not exist.");
            }
            else
            {
                if (User.IsInRole("ADMIN") || User.Identity.Name == order.email)
                {
                    return Ok(order);
                }
            }

            return BadRequest("Authorization Denied! Only admin or the order owner allowed!");
        }

        // GET: api/Orders/byemail
        [Route("byemail")]
        [HttpGet]
        public IHttpActionResult GetOrdersByEmail(string email)
        {
            if (User.IsInRole("ADMIN") || User.Identity.Name == email)
            {
                var orders = db.Orders.Where(o => o.email == email);
                if (orders == null)
                {
                    return NotFound();
                }
                return Ok(orders.ToList());
            }

            return BadRequest("Authorization Denied! Only admin or the order owner allowed!");

        }

        // PUT: api/Orders/close
        [ResponseType(typeof(string))]
        [Route("close")]
        [HttpPut]
        public IHttpActionResult CloseOrder(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return Content(HttpStatusCode.NotFound, "Order does not exist.");
            }
            else
            {
                if (User.IsInRole("ADMIN") || User.Identity.Name == order.email)
                {
                    if (order.precoFrete != 0)
                    {
                        order.status = "fechado";
                        db.SaveChanges();
                        return Ok("Order is closed!");
                    }
                    else
                    {
                        return BadRequest("Cannot close the order. The shipping freight was not calculated yet!");
                    }
                }
            }

            return BadRequest("Authorization Denied! Only admin or the order owner allowed!");

        }

        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (User.IsInRole("ADMIN") || (order.email != null && order.email == User.Identity.Name))
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != order.Id)
                {
                    return BadRequest();
                }

                db.Entry(order).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(id))
                    {
                        return Content(HttpStatusCode.NotFound, "Order does not exist.");
                    }
                    else
                    {
                        throw;
                    }
                }

                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest("Authorization Denied! Only admin or the order owner allowed!");

        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            order.status = "novo";
            order.pesoTotal = 0;
            order.precoFrete = 0;
            order.precoTotal = 0;
            order.dataPedido = DateTime.Now;
            order.email = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return Content(HttpStatusCode.NotFound, "Order does not exist.");
            }
            else
            {
                if (User.IsInRole("ADMIN") || User.Identity.Name == order.email)
                {
                    db.Orders.Remove(order);
                    db.SaveChanges();

                    return Ok(order);
                }
            }
            return BadRequest("Authorization Denied! Only admin or the order owner allowed!");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}
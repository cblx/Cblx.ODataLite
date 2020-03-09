using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cblx.ODataLite
{
    public static class ODataHelpers
    {
        public static ODataResult<T> Execute<T>(this IQueryable<T> queryable, IODataParameters oDataParameters)
        {
            var list = oDataParameters.Top == 0 ? new List<T>() : ExecuteList(queryable, oDataParameters);
            var count = oDataParameters.Count.GetValueOrDefault() ? queryable.Count() : default(int?);
            var result = new ODataResult<T>(list, count, oDataParameters);
            return result;
        }

        private static List<T> ExecuteList<T>(this IQueryable<T> queryable, IODataParameters oDataParameters)
        {
            if (string.IsNullOrWhiteSpace(oDataParameters.Select)) { throw new ArgumentException("Selecting members is required!"); }


            //queryable = Select(queryable, query);

            //Order by
            if (!string.IsNullOrWhiteSpace(oDataParameters.OrderBy))
            {
                var methods = typeof(Queryable).GetMethods();
                var defMethodOrderBy = methods.FirstOrDefault(m => m.Name == nameof(Queryable.OrderBy));
                var defMethodOrderByDescending = methods.FirstOrDefault(m => m.Name == nameof(Queryable.OrderByDescending));

                //Cria param para o item
                var paramItem = Expression.Parameter(typeof(T), "item");

                var orderByes = oDataParameters.OrderBy.Split(',');
                foreach (var ob in orderByes)
                {
                    var parts = ob.Split(' ');

                    //Procuro a prop correspondente
                    var prop = typeof(T).GetProperties().FirstOrDefault(prop2 => string.Equals(parts[0], prop2.Name, StringComparison.OrdinalIgnoreCase));
                    if (prop == null)
                    {
                        throw new ArgumentException($"Property '{parts[0]}' used in $orderby was not found");
                    }

                    //Crio um lambda de acesso a prop
                    var exp = Expression.Lambda(Expression.Property(paramItem, prop), paramItem);

                    var methodOrderBy = defMethodOrderBy.MakeGenericMethod(new Type[] { typeof(T), prop.PropertyType });
                    var methodOrderByDescending = defMethodOrderByDescending.MakeGenericMethod(new Type[] { typeof(T), prop.PropertyType });
                    //Acho que vai acabar suportando 1 order by, creio que precise utrilizaro o thenby se for multiplos
                    if (parts.Length > 1 && parts.Last().ToLower() == "desc")
                    {
                        queryable = (IQueryable<T>)methodOrderByDescending.Invoke(null, new object[] { queryable, exp });
                    }
                    else
                    {
                        queryable = (IQueryable<T>)methodOrderBy.Invoke(null, new object[] { queryable, exp });
                    }
                }
            }

            //Seleciona os campos
            queryable = Select(queryable, oDataParameters);


            //Skip
            if (oDataParameters.Skip.HasValue)
            {
                queryable = queryable.Skip(oDataParameters.Skip.Value);
            }

            //Take
            if (oDataParameters.Top.HasValue)
            {
                queryable = queryable.Take(oDataParameters.Top.Value);
            }

            try
            {
                //Lista dos itens pós execução
                //var executed = selectedQueryable.ToList();
                var executed = queryable.ToList();

                return executed;
            }
            catch (NullReferenceException ex)
            {
                throw new Exception("Não foi possível executar a consulta. Verifique se não há possibilidade de referência nula em sua projeção.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Não foi possível executar a consulta. Verifique os mapeamentos dos campos de forma que todos possam ser mapeados para SQL.", ex);
            }
        }


        //Aplica o select com projeção para o mesmo tipo
        public static IQueryable<T> Select<T>(this IQueryable<T> queryable, IODataParameters oDataParameters)
        {
            return Select<T, T>(queryable, oDataParameters);
        }

        //Aplica o select com projeção para outro tipo
        public static IQueryable<TTarget> Select<T, TTarget>(this IQueryable<T> queryable, IODataParameters oDataParameters)
        {
            //Cria param para o item
            var paramItem = Expression.Parameter(typeof(T), "item");

            //Aplicar o Select
            var exp = Expression.Lambda<Func<T, TTarget>>(
                            Expression.MemberInit(
                                Expression.New(typeof(TTarget).GetConstructor(new Type[0])),
                                oDataParameters.Select.Split(',')
                                    .Select(
                                    str =>
                                    {
                                        var prop = typeof(TTarget).GetProperties().FirstOrDefault(p => p.Name.ToLower() == str.ToLower());
                                        if (prop == null)
                                        {
                                            throw new ArgumentException($"Property '{str}' not found");
                                        }
                                        return Expression.Bind(
                                            prop,
                                            Expression.Property(paramItem, prop)
                                        );
                                    }
                                )
                            ),
                            paramItem
                        );
            var qry = queryable.Select(exp);
            return qry;
        }
    }
}

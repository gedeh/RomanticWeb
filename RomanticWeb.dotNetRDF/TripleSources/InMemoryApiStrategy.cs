﻿using System;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Entities;
using RomanticWeb.Ontologies;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace RomanticWeb.DotNetRDF.TripleSources
{
	internal class InMemoryApiStrategy:IStoreQueryStrategy
	{
		private static readonly NodeFactory NodeFactory=new NodeFactory();
		private readonly IInMemoryQueryableStore _tripleStore;

		public InMemoryApiStrategy(IInMemoryQueryableStore tripleStore)
		{
			_tripleStore=tripleStore;
		}

		public IEnumerable<RdfNode> GetObjectsForPredicate(EntityId entityId,Uri predicate)
		{
			INode entityNode=entityId.ToNode(NodeFactory);
			INode predicateNode=NodeFactory.CreateUriNode(predicate);

			return _tripleStore.GetTriplesWithSubjectPredicate(entityNode,predicateNode)
							   .Select(t => t.Object.WrapNode());
		}

		public bool TryGetListElements(RdfNode rdfList,out IEnumerable<RdfNode> listElements)
		{
			IGraph graph=_tripleStore[rdfList.GraphUri];
			IBlankNode listNode=graph.GetBlankNode(rdfList.BlankNodeId);
			IUriNode rdfFirst=graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));

			if (graph.GetTriplesWithSubjectPredicate(listNode,rdfFirst).Any())
			{
				listElements=graph.GetListItems(listNode).Select(n => n.WrapNode()).ToList();
				return true;
			}

			listElements=null;
			return false;
		}

		public ITripleStore GetNodesForQuery(SparqlQuery query)
		{
			ITripleStore result=new TripleStore();
			IGraph graph=null;
			InMemoryDataset dataSet=new InMemoryDataset(_tripleStore);
			ISparqlQueryProcessor processor=new LeviathanQueryProcessor(dataSet);
			object results=processor.ProcessQuery(query);
			if (results is IGraph)
			{
				graph=(IGraph)results;
			}
			else if (results is SparqlResultSet)
			{
				graph=new Graph();
				string subjectVariable=null;
				string predicateVariable=null;
				string objectVariable=null;

				foreach (string variable in ((SparqlResultSet)results).Variables)
				{
					if (subjectVariable==null)
					{
						subjectVariable=variable;
					}
					else if (predicateVariable==null)
					{
						predicateVariable=variable;
					}
					else if (objectVariable==null)
					{
						objectVariable=variable;
					}
					else
					{
						break;
					}
				}

				if ((subjectVariable!=null)&&(predicateVariable!=null)&&(objectVariable!=null))
				{
					graph=new Graph(((SparqlResultSet)results).ToTripleCollection(graph,subjectVariable,predicateVariable,objectVariable));
				}
			}

			if (graph!=null)
			{
				result.Add(graph);
			}

			return result;
		}
	}
}
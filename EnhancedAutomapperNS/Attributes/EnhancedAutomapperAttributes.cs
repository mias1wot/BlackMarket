using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;

namespace BlackMarket_API.Attributes
{
	public class EnhancedAutomapperAttributes
	{
		public enum Operation
		{
			From,
			GroupBy
		}


		//When a field is not contained in DB directly and will be calculated later, but for this 'fieldName' DB field is required
		//So 'fieldName' will be included in the result
		public class FormedFrom : Attribute
		{
			public string DbFieldName { get; set; }
		}


		//Represents LINQ GroupJoin operation
		//Here goes List of additional operations which will be done over InnerCollection (i.e. find Sum of aggregated 'fieldName' field)
		public class GroupJoin : Attribute
		{
			public enum Operations
			{
				Sum,
				Select,
				Contains
			}
			//FROM <Initial> JOIN <JoinTo> ON <JoinFromField> = <JoinToField>

			//What table to join
			public string JoinTo { get; set; }

			//Initial table join field (on the left in ON statement)
			public string JoinFromField { get; set; }

			//JoinTo table join field (on the right in ON statement)
			public string JoinToField { get; set; }

			//These actions will be done after join and group by over the collection of aggregated rows of JoinTo table (aggregated by JoinToField)
			public List<(Operations Operation, string FieldName)> ActionsOverInnerCollection { get; set; } = new List<(Operations, string)>();


			//public GroupJoin(string joinTo, string joinFromField, string joinToField, List<(Operations, string)> actionsOverInnerCollection)
			//actionsOverInnerCollection is a sequence of [GroupJoin.Operations, fieldName], repeated as many time as needed to specify all required actions
			//public GroupJoin(string joinTo, string joinFromField, string joinToField, params object[] actionsOverInnerCollection)
			//{
			//	JoinTo = joinTo;
			//	JoinFromField = joinFromField;
			//	JoinToField = joinToField;
			//	try
			//	{
			//		for (int i = 0; i < actionsOverInnerCollection.Length; i += 2)
			//		{
			//			ActionsOverInnerCollection.Add(
			//				((Operations)actionsOverInnerCollection[i],
			//				(string)actionsOverInnerCollection[i + 1]));
			//		}
			//	}
			//	catch (Exception e)
			//	{

			//	}
			//	ActionsOverInnerCollection = actionsOverInnerCollection;
			//}

			public GroupJoin(string joinTo, string joinFromField, string joinToField)
			{
				JoinTo = joinTo;
				JoinFromField = joinFromField;
				JoinToField = joinToField;
			}


			//Multiple instances of this class represent actions that will be done over the collection of aggregated rows
			//These instances will join the GroupJoin parent class and will be written to property 'ActionsOverInnerCollection'
			[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
			public class ActionOverInnerCollection : Attribute
			{
				public Operations Operation { get; set; }
				public string FieldName { get; set; }

				//Info below is used to join multiple attributes of this class to single parent GroupJoin class which will have aggregated innerCollection
				public Type CallerClassType { get; set; }
				public PropertyInfo CallerProperty { get; set; }
				public ActionOverInnerCollection(Operations operation, string fieldName, Type classType, [CallerMemberName] string propertyName = null)
				{
					var CallerProperty = classType.GetProperty(propertyName);
					CallerClassType = classType;
					//var attrs = property.GetCustomAttributes<ActionOverInnerCollection>(true);

					//var ttt2242 = Type.GetType("BlackMarket_API.Data.ViewModels.TestProductViewModel");


					Operation = operation;
					FieldName = fieldName;
					//ActionsOverInnerCollection.Add((Operations, FieldName));
				}
			}
		}
	}
}
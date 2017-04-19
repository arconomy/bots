using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using Microsoft.VisualBasic;
using System.Xml; 

namespace Niffler.Data
{

    public class Objects
    {

        public static Hashtable ToHashTable<t>(t Obj, string Operation = "")
        {

            Hashtable Parameters = new Hashtable();
            if (!string.IsNullOrEmpty(Operation))
                Parameters.Add("Operation", Operation);

            if (Obj != null)
            {

                foreach (System.Reflection.PropertyInfo Pi in typeof(t).GetProperties())
                {
                    if (Pi.GetValue(Obj) != null)
                    {
                        // if they are readonly then no point sending them to the db... 
                        if (Pi.CanWrite)
                        {
                            DateTime dte = DateTime.MinValue;
                            if (IsDate(Pi.GetValue(Obj).ToString()))
                            {
                                if (!((DateTime)Pi.GetValue(Obj) == DateTime.MinValue))
                                {
                                    Parameters.Add(Pi.Name, Pi.GetValue(Obj));
                                }
                            }
                            else if (object.ReferenceEquals(Pi.PropertyType, typeof(Guid)))
                            {
                                Guid tmpID = (Guid)Pi.GetValue(Obj);
                                if (tmpID != Guid.Empty)
                                    Parameters.Add(Pi.Name, Pi.GetValue(Obj));

                            }
                            else if (object.ReferenceEquals(Pi.PropertyType, typeof(TimeSpan)))
                            {
                                Parameters.Add(Pi.Name, ((TimeSpan)Pi.GetValue(Obj)).ToString());

                            }
                            else if (Pi.PropertyType.IsEnum)
                            {
                                Parameters.Add(Pi.Name, System.Enum.GetName(Pi.PropertyType, Pi.GetValue(Obj)));


                            }
                            else
                            {
                                Parameters.Add(Pi.Name, Pi.GetValue(Obj));
                            }
                        }
                    }
                }
            }

            return Parameters;

        }

          public static bool IsDate(object item)
        {
            try
            {
                DateTime X = (DateTime)item;
                    return true;
            }
            catch (Exception)
            {
                
               return false;
            }


        }


        public static t Copy<t>(object item)
        {

            t obj = Activator.CreateInstance<t>();

            foreach (PropertyInfo P in item.GetType().GetProperties())
            {
                foreach (PropertyInfo objP in obj.GetType().GetProperties())
                {
                    if (P.Name == objP.Name & objP.CanWrite)
                    {
                        objP.SetValue(obj, P.GetValue(item));
                    }
                }
            }

            return obj;

        }

         

    }



}
﻿using DBHelper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace FestivalRegistratie.Models.DAL
{
    public class OrderRepository
    {
        public List<TicketType> tickettypes { get; set; }
        public Reservation reservation { get; set; }
        public static List<TicketType> GetTickets()
        {
            List<TicketType> types = new List<TicketType>();
            string sql = "SELECT * FROM tickettype";
            DbDataReader typesreader = Database.GetData(sql);
            while (typesreader.Read())
            {
                types.Add(MakeTicket(typesreader));
            }
            typesreader.Close();
            return types;
        }
        private static TicketType MakeTicket(DbDataReader typesreader)
        {
            TicketType t = new TicketType();
            t.Id = typesreader["Id"].ToString();
            t.Available = typesreader["Available"].ToString();
            t.Id = typesreader["Id"].ToString();
            t.Name = typesreader["TicketName"].ToString();
            t.Price = typesreader["price"].ToString();
            int reserved = NumberOfReservedTickets(t.Id);
            int startavailable = Convert.ToInt32(t.Available);
            t.Available = Convert.ToString(startavailable - reserved);
            return t;
        }
        public static int NumberOfReservedTickets(string TicketID)
        {
            string sql = "SELECT SUM(Number) AS NumberOfReservedTickets FROM orderedtickets WHERE TicketType = @tID";
            DbParameter tID = Database.AddParameter("@tID", TicketID);
            DbDataReader reader = Database.GetData(sql, tID);
            reader.Read();
            int ireserved = 0;
            Int32.TryParse(reader["NumberOfReservedTickets"].ToString(), out ireserved);
            reader.Close();
            return ireserved;
        }
        internal static TicketType GetTicket(string TicketTypeID)
        {
            string sql = "SELECT * FROM tickettype WHERE Id = @id";
            DbParameter id = Database.AddParameter("@id", TicketTypeID);
            DbDataReader reader = Database.GetData(sql, id);
            reader.Read();
            TicketType t =  MakeTicket(reader);
            reader.Close();
            return t;
        }
        internal static Reservation UpdateReservationWithUserData(string UserName, string ttID)
        {
            Reservation r = new Reservation();
            r.Ticket = GetTicket(ttID);
            string sql = "SELECT * FROM UserProfile WHERE UserName = @UN";
            DbParameter un = Database.AddParameter("@UN", UserName);
            DbDataReader reader = Database.GetData(sql, un);
            reader.Read();
            r.Email = reader["Email"].ToString();
            r.FirstName = reader["UserFirstName"].ToString();
            r.LastName = reader["UserLastName"].ToString();
            reader.Close();
            return r;
        }
        internal static Reservation SaveTicket(string Number, string Id, string p)
        {
           // Number = reserved tickets
            // Id = ticket Id
            // p = UserName user
            Reservation r = new Reservation();
            r = UpdateReservationWithUserData(p, Id);
            r.Number = Convert.ToInt32(Number);
            r.TotalCost = r.Number * Convert.ToInt32(r.Ticket.Price);
            int result = Insert(r);
            return r;
        }
        public static int Insert(Reservation reservation)
        {
            int affected = 0;
            
                string sql = "INSERT INTO orderedtickets (Holder, HolderLast, Email, TicketType,Number) values (@h, @hl,@e,@tt, @N)";
                DbParameter h = Database.AddParameter("@h", reservation.FirstName);
                DbParameter hl = Database.AddParameter("@hl", reservation.LastName);
                DbParameter e = Database.AddParameter("@e", reservation.Email);
                DbParameter tt = Database.AddParameter("@tt", reservation.Ticket.Id);
                DbParameter n = Database.AddParameter("@n", reservation.Number);
                affected += Database.ModifyData(sql, h, hl, e, tt,n);
            
            return affected;
        }
       
    }
}
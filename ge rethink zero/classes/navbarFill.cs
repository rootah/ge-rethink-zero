using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraBars;
using DevExpress.XtraNavBar;
using ge_rethink_zero.forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ge_rethink_zero.classes
{
    public class navbarFill
    {
        //private static IMongoClient _client;
        //private static IMongoDatabase _database;

        // note Nav Bar Fill 
        //private async void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    //var groupcollection = _database.GetCollection<BsonDocument>("groups");
        //    //var groupprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
        //    //var groupsort = Builders<BsonDocument>.Sort.Ascending("groupno");

        //    //var stdcollection = _database.GetCollection<BsonDocument>("students");
        //    //var stdprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("fullname");
        //    //var stdsort = Builders<BsonDocument>.Sort.Ascending("lname");

        //    //var groupcursor = await groupcollection.Find(new BsonDocument()).Project(groupprojection).Sort(groupsort).ToCursorAsync();

        //    //foreach (var document in groupcursor.ToEnumerable())
        //    //{
        //    //    var newgroup = mainForm.navBarControl1.Groups.Add();
        //    //    newgroup.Caption = document.Values.Single().ToString();

        //    //    var stdfilter = Builders<BsonDocument>.Filter.Eq("groupno", document.Values.Single().ToString());
        //    //    var stdcursor = stdcollection.Find(stdfilter).Project(stdprojection).Sort(stdsort).ToCursor();

        //    //    foreach (var stddocument in stdcursor.ToEnumerable())
        //    //    {
        //    //        NavBarItem indexItem = mainForm.navBarControl1.Items.Add();
        //    //        indexItem.Caption = stddocument.Values.Single().ToString();
        //    //        newgroup.ItemLinks.Add(indexItem);
        //    //    }
        //    //}
        //}

        //private async void fillnavBtn_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    treeList1.BeginUnboundLoad();

        //    var groupcollection = _database.GetCollection<BsonDocument>("groups");
        //    var groupprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
        //    var groupsort = Builders<BsonDocument>.Sort.Ascending("groupno");
        //    var groupcursor = await groupcollection.Find(new BsonDocument()).Project(groupprojection).Sort(groupsort).ToCursorAsync();

        //    var stdcollection = _database.GetCollection<BsonDocument>("students");
        //    var stdprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("fullname");
        //    var stdsort = Builders<BsonDocument>.Sort.Ascending("lname");

        //    foreach (var document in groupcursor.ToEnumerable())
        //    {
        //        TreeListNode parentForRootNodes = null;
        //        var rootNode = treeList1.AppendNode(new object[] { document.Values.Single().ToString() }, parentForRootNodes);

        //        var stdfilter = Builders<BsonDocument>.Filter.Eq("groupno", document.Values.Single().ToString());
        //        var stdcursor = stdcollection.Find(stdfilter).Project(stdprojection).Sort(stdsort).ToCursor();

        //        foreach (var stddocument in stdcursor.ToEnumerable())
        //        {
        //            treeList1.AppendNode(new object[] { stddocument.Values.Single().ToString() }, rootNode);
        //        }
        //    }

        //    treeList1.EndUnboundLoad();
        //}
    }
}

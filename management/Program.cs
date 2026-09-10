using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace DROPLINK.Management;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class Delivery
{
    public string Id { get; set; } = "";
    public string Customer { get; set; } = "";
    public string Pickup { get; set; } = "";
    public string Dropoff { get; set; } = "";
    public string Driver { get; set; } = "Unassigned";
    public string Status { get; set; } = "New Order";
    public decimal Fare { get; set; }
    public decimal Tip { get; set; }
    public string Payment { get; set; } = "Cash";
}

public class DriverInfo
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Vehicle { get; set; } = "";
    public string Status { get; set; } = "Offline";
    public int ActiveJobs { get; set; }
    public decimal Rating { get; set; }
}

public sealed class MainForm : Form
{
    readonly Color Navy = Color.FromArgb(0, 43, 103);
    readonly Color NavyDark = Color.FromArgb(0, 26, 69);
    readonly Color Gold = Color.FromArgb(224, 164, 0);
    readonly Color GoldBright = Color.FromArgb(255, 201, 40);
    readonly Color Bg = Color.FromArgb(244, 247, 251);
    readonly Color Ink = Color.FromArgb(23, 34, 52);
    readonly Color Muted = Color.FromArgb(104, 116, 137);

    readonly Panel content = new() { Dock = DockStyle.Fill };
    readonly Label pageTitle = new() { AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold) };
    DataGridView? orderGrid;

    readonly List<Delivery> deliveries = new()
    {
        new() { Id="DLK-C204181", Customer="Naledi M.", Pickup="Maponya Mall, Soweto", Dropoff="Pimville Zone 9", Driver="Thabo M.", Status="On the way", Fare=85, Tip=20, Payment="Card" },
        new() { Id="DLK-C204226", Customer="John Doe", Pickup="Orlando West", Dropoff="Protea Glen", Driver="Unassigned", Status="New Order", Fare=65, Tip=10, Payment="Cash" },
        new() { Id="DLK-C204261", Customer="Precious K.", Pickup="Johannesburg CBD", Dropoff="Diepkloof", Driver="Sipho N.", Status="Driver going to pickup", Fare=115, Tip=30, Payment="Card" },
        new() { Id="DLK-C204299", Customer="Mandla S.", Pickup="Rosebank", Dropoff="Meadowlands", Driver="Unassigned", Status="Searching for driver", Fare=135, Tip=0, Payment="Cash" },
        new() { Id="DLK-C203990", Customer="Lerato P.", Pickup="Braamfontein", Dropoff="Dobsonville", Driver="Thabo M.", Status="Delivered", Fare=95, Tip=20, Payment="Card" }
    };

    readonly List<DriverInfo> drivers = new()
    {
        new() { Name="Thabo M.", Phone="071 111 2200", Vehicle="Toyota Etios • GP", Status="Online", ActiveJobs=1, Rating=4.9m },
        new() { Name="Sipho N.", Phone="072 222 3300", Vehicle="Suzuki Dzire • GP", Status="Online", ActiveJobs=1, Rating=4.8m },
        new() { Name="Kabelo R.", Phone="073 333 4400", Vehicle="Hyundai Grand i10 • GP", Status="Online", ActiveJobs=0, Rating=4.7m },
        new() { Name="Vusi T.", Phone="074 444 5500", Vehicle="Toyota Corolla Quest • GP", Status="Break", ActiveJobs=0, Rating=4.8m },
        new() { Name="Mpho D.", Phone="076 555 6600", Vehicle="Renault Triber • GP", Status="Offline", ActiveJobs=0, Rating=4.6m }
    };

    public MainForm()
    {
        Text = "DROPLINK Management Control";
        Width = 1420;
        Height = 860;
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Bg;
        Font = new Font("Segoe UI", 10);

        var sidebar = BuildSidebar();
        var main = new Panel { Dock = DockStyle.Fill, BackColor = Bg };
        main.Controls.Add(content);
        main.Controls.Add(BuildTopbar());
        Controls.Add(main);
        Controls.Add(sidebar);
        ShowDashboard();
    }

    Panel BuildSidebar()
    {
        var p = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = NavyDark, Padding = new Padding(14) };
        var logo = new PictureBox { Width = 72, Height = 72, SizeMode = PictureBoxSizeMode.Zoom, Left = 18, Top = 18 };
        logo.Image = LoadLogo();
        var brand = new Label { Text = "DROPLINK", ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Left = 18, Top = 96 };
        var sub = new Label { Text = "MANAGEMENT CONTROL", ForeColor = Color.FromArgb(185, 205, 230), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Left = 20, Top = 132 };
        p.Controls.Add(logo); p.Controls.Add(brand); p.Controls.Add(sub);

        var y = 180;
        foreach (var item in new (string Text, string Icon, Action Click)[]
        {
            ("Dashboard", "⌂", ShowDashboard), ("Orders", "▤", ShowOrders), ("Drivers", "●", ShowDrivers),
            ("Customers", "◉", ShowCustomers), ("Finance", "R", ShowFinance), ("Settings", "⚙", ShowSettings)
        })
        {
            var b = new Button { Text = $"  {item.Icon}   {item.Text}", Width = 200, Height = 46, Left = 0, Top = y, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White, BackColor = NavyDark, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Navy;
            b.Click += (_,__) => item.Click();
            p.Controls.Add(b); y += 51;
        }
        var online = new Label { Text = "●  CONTROL ONLINE", ForeColor = Color.FromArgb(72, 220, 120), AutoSize = true, Left = 20, Bottom = 25, Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        p.Controls.Add(online);
        return p;
    }

    Panel BuildTopbar()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Color.White, Padding = new Padding(24, 16, 24, 12) };
        pageTitle.ForeColor = Ink;
        pageTitle.Location = new Point(24, 20);
        var status = new Label { Text = "● SYSTEM ONLINE", ForeColor = Color.FromArgb(22, 128, 71), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        status.Location = new Point(top.Width - 370, 26);
        status.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var admin = new Button { Text = "👤  Management", Width = 150, Height = 38, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Navy, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        admin.Location = new Point(top.Width - 178, 16); admin.Anchor = AnchorStyles.Top | AnchorStyles.Right; admin.FlatAppearance.BorderColor = Color.FromArgb(220,225,234);
        top.Controls.Add(pageTitle); top.Controls.Add(status); top.Controls.Add(admin);
        return top;
    }

    Image? LoadLogo()
    {
        try
        {
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("DROPLINK.Management.deutronoma_symbol.png");
            if (s == null) return null;
            using var img = Image.FromStream(s);
            return new Bitmap(img);
        }
        catch { return null; }
    }

    void SetPage(string title, Control body)
    {
        pageTitle.Text = title;
        content.Controls.Clear();
        body.Dock = DockStyle.Fill;
        content.Controls.Add(body);
    }

    Panel PagePanel() => new() { BackColor = Bg, Padding = new Padding(24) };

    Panel StatCard(string title, string value, string detail)
    {
        var p = new Panel { Width = 210, Height = 112, BackColor = Color.White, Margin = new Padding(0,0,14,14), Padding = new Padding(16) };
        p.Controls.Add(new Label { Text = title.ToUpperInvariant(), ForeColor = Muted, AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(16,14) });
        p.Controls.Add(new Label { Text = value, ForeColor = Navy, AutoSize = true, Font = new Font("Segoe UI", 23, FontStyle.Bold), Location = new Point(14,34) });
        p.Controls.Add(new Label { Text = detail, ForeColor = Muted, AutoSize = true, Font = new Font("Segoe UI", 8), Location = new Point(16,79) });
        return p;
    }

    void ShowDashboard()
    {
        var root = PagePanel();
        var stats = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 132, BackColor = Bg, WrapContents = false, AutoScroll = true };
        stats.Controls.Add(StatCard("New Orders", deliveries.Count(d => d.Status is "New Order" or "Searching for driver").ToString(), "Waiting for assignment"));
        stats.Controls.Add(StatCard("Active Deliveries", deliveries.Count(d => d.Status != "Delivered" && d.Driver != "Unassigned").ToString(), "Currently moving"));
        stats.Controls.Add(StatCard("Online Drivers", drivers.Count(d => d.Status == "Online").ToString(), "Ready / active"));
        stats.Controls.Add(StatCard("Today Revenue", "R" + deliveries.Sum(d => d.Fare).ToString("0"), "Delivery fees"));
        stats.Controls.Add(StatCard("Driver Tips", "R" + deliveries.Sum(d => d.Tip).ToString("0"), "Customer tips"));

        var title = new Label { Text = "Live Deliveries", Dock = DockStyle.Top, Height = 38, ForeColor = Navy, Font = new Font("Segoe UI", 13, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        var grid = BuildOrdersGrid(deliveries.Take(5).ToList());
        grid.Dock = DockStyle.Fill;
        root.Controls.Add(grid); root.Controls.Add(title); root.Controls.Add(stats);
        SetPage("Dashboard", root);
    }

    DataGridView BaseGrid()
    {
        var g = new DataGridView { BackgroundColor = Color.White, BorderStyle = BorderStyle.None, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 42, RowTemplate = { Height = 42 } };
        g.ColumnHeadersDefaultCellStyle.BackColor = Navy;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(229, 238, 250);
        g.DefaultCellStyle.SelectionForeColor = Ink;
        g.DefaultCellStyle.ForeColor = Ink;
        g.GridColor = Color.FromArgb(232,236,242);
        return g;
    }

    DataGridView BuildOrdersGrid(List<Delivery> list)
    {
        var g = BaseGrid();
        foreach (var h in new[] { "Order ID", "Customer", "Pickup", "Drop-off", "Driver", "Status", "Fare", "Tip" }) g.Columns.Add(h, h);
        foreach (var d in list)
        {
            var i = g.Rows.Add(d.Id, d.Customer, d.Pickup, d.Dropoff, d.Driver, d.Status, "R" + d.Fare.ToString("0"), "R" + d.Tip.ToString("0"));
            g.Rows[i].Tag = d;
        }
        g.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && g.Rows[e.RowIndex].Tag is Delivery d) OpenTrip(d); };
        return g;
    }

    void ShowOrders()
    {
        var root = PagePanel();
        var tools = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, BackColor = Bg, WrapContents = false };
        var search = new TextBox { Width = 250, Height = 36, PlaceholderText = "Search order, customer or driver...", Margin = new Padding(0,8,10,8) };
        var status = new ComboBox { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0,8,10,8) };
        status.Items.AddRange(new object[] { "All statuses", "New Order", "Searching for driver", "Driver going to pickup", "On the way", "Driver arrived", "Delivered" }); status.SelectedIndex = 0;
        var assign = ActionButton("Assign Driver", Gold, Ink);
        var change = ActionButton("Change Status", Navy, Color.White);
        var open = ActionButton("Open Trip", Color.White, Navy); open.FlatAppearance.BorderColor = Navy; open.FlatAppearance.BorderSize = 1;
        var create = ActionButton("+ New Delivery", NavyDark, Color.White);
        tools.Controls.Add(search); tools.Controls.Add(status); tools.Controls.Add(assign); tools.Controls.Add(change); tools.Controls.Add(open); tools.Controls.Add(create);

        orderGrid = BuildOrdersGrid(deliveries);
        orderGrid.Dock = DockStyle.Fill;
        void Refresh()
        {
            var q = search.Text.Trim().ToLowerInvariant();
            var s = status.SelectedItem?.ToString() ?? "All statuses";
            var filtered = deliveries.Where(d => (s == "All statuses" || d.Status == s) && (q.Length == 0 || $"{d.Id} {d.Customer} {d.Driver} {d.Pickup} {d.Dropoff}".ToLowerInvariant().Contains(q))).ToList();
            orderGrid!.Parent?.Controls.Remove(orderGrid);
            orderGrid = BuildOrdersGrid(filtered); orderGrid.Dock = DockStyle.Fill; root.Controls.Add(orderGrid); orderGrid.BringToFront(); tools.BringToFront();
        }
        search.TextChanged += (_,__) => Refresh(); status.SelectedIndexChanged += (_,__) => Refresh();
        assign.Click += (_,__) => { var d = SelectedDelivery(); if (d != null) AssignDriver(d); };
        change.Click += (_,__) => { var d = SelectedDelivery(); if (d != null) ChangeStatus(d); };
        open.Click += (_,__) => { var d = SelectedDelivery(); if (d != null) OpenTrip(d); };
        create.Click += (_,__) => CreateDelivery();
        root.Controls.Add(orderGrid); root.Controls.Add(tools);
        SetPage("Orders", root);
    }

    Button ActionButton(string text, Color bg, Color fg) => new() { Text=text, Width=125, Height=36, BackColor=bg, ForeColor=fg, FlatStyle=FlatStyle.Flat, Margin=new Padding(0,8,10,8), Cursor=Cursors.Hand, Font=new Font("Segoe UI",9,FontStyle.Bold) };

    Delivery? SelectedDelivery() => orderGrid?.SelectedRows.Count > 0 ? orderGrid.SelectedRows[0].Tag as Delivery : null;

    void AssignDriver(Delivery d)
    {
        using var f = SmallDialog("Assign Driver", 420, 230);
        var cb = new ComboBox { Left=28, Top=62, Width=340, DropDownStyle=ComboBoxStyle.DropDownList };
        cb.Items.AddRange(drivers.Where(x=>x.Status=="Online").Select(x=>x.Name).Cast<object>().ToArray()); if(cb.Items.Count>0) cb.SelectedIndex=0;
        var ok = DialogButton("Assign", 28, 118, Gold, Ink);
        ok.Click += (_,__) => { if(cb.SelectedItem!=null){ d.Driver=cb.SelectedItem.ToString()!; if(d.Status is "New Order" or "Searching for driver") d.Status="Driver assigned"; f.DialogResult=DialogResult.OK; } };
        f.Controls.Add(new Label { Text=$"Order {d.Id}", Left=28, Top=24, AutoSize=true, ForeColor=Navy, Font=new Font("Segoe UI",11,FontStyle.Bold)}); f.Controls.Add(cb); f.Controls.Add(ok);
        if(f.ShowDialog()==DialogResult.OK) ShowOrders();
    }

    void ChangeStatus(Delivery d)
    {
        using var f = SmallDialog("Change Delivery Status", 430, 240);
        var cb = new ComboBox { Left=28, Top=64, Width=345, DropDownStyle=ComboBoxStyle.DropDownList };
        cb.Items.AddRange(new object[] { "New Order", "Searching for driver", "Driver assigned", "Driver going to pickup", "Parcel collected", "On the way", "Driver arrived", "Delivered", "Issue" });
        cb.SelectedItem=d.Status;
        var ok=DialogButton("Update",28,124,Navy,Color.White); ok.Click += (_,__)=>{ if(cb.SelectedItem!=null){d.Status=cb.SelectedItem.ToString()!;f.DialogResult=DialogResult.OK;} };
        f.Controls.Add(new Label { Text=d.Id, Left=28, Top=25, AutoSize=true, ForeColor=Navy, Font=new Font("Segoe UI",11,FontStyle.Bold)}); f.Controls.Add(cb); f.Controls.Add(ok);
        if(f.ShowDialog()==DialogResult.OK) ShowOrders();
    }

    void CreateDelivery()
    {
        using var f=SmallDialog("Create Delivery",540,465);
        var customer=Input(f,"Customer",28,55); var pickup=Input(f,"Pickup address",28,120); var drop=Input(f,"Drop-off address",28,185); var fare=Input(f,"Fare",28,250); var tip=Input(f,"Tip",270,250);
        var save=DialogButton("Create Order",28,330,Gold,Ink); save.Width=220;
        save.Click += (_,__)=>{ if(string.IsNullOrWhiteSpace(customer.Text)||string.IsNullOrWhiteSpace(pickup.Text)||string.IsNullOrWhiteSpace(drop.Text)) return; decimal.TryParse(fare.Text,out var ff); decimal.TryParse(tip.Text,out var tt); deliveries.Insert(0,new Delivery{Id="DLK-M"+DateTime.Now.ToString("HHmmss"),Customer=customer.Text,Pickup=pickup.Text,Dropoff=drop.Text,Fare=ff,Tip=tt,Status="New Order"}); f.DialogResult=DialogResult.OK; };
        f.Controls.Add(save); if(f.ShowDialog()==DialogResult.OK) ShowOrders();
    }

    TextBox Input(Form f,string label,int x,int y)
    {
        f.Controls.Add(new Label{Text=label,Left=x,Top=y-22,AutoSize=true,ForeColor=Muted,Font=new Font("Segoe UI",8,FontStyle.Bold)});
        var t=new TextBox{Left=x,Top=y,Width=240,Height=32};f.Controls.Add(t);return t;
    }

    Form SmallDialog(string title,int w,int h) => new(){Text=title,Width=w,Height=h,StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,BackColor=Bg,Font=new Font("Segoe UI",10)};
    Button DialogButton(string text,int x,int y,Color bg,Color fg){var b=new Button{Text=text,Left=x,Top=y,Width=150,Height=40,BackColor=bg,ForeColor=fg,FlatStyle=FlatStyle.Flat,Font=new Font("Segoe UI",9,FontStyle.Bold)};b.FlatAppearance.BorderSize=0;return b;}

    void OpenTrip(Delivery d)
    {
        using var f = new Form { Text=$"Trip {d.Id}", Width=920, Height=650, StartPosition=FormStartPosition.CenterParent, BackColor=Bg, Font=new Font("Segoe UI",10) };
        var left = new Panel { Dock=DockStyle.Left, Width=390, BackColor=Color.White, Padding=new Padding(22) };
        var info = new Label { Dock=DockStyle.Top, Height=360, ForeColor=Ink, Font=new Font("Segoe UI",10), Text=$"ORDER\n{d.Id}\n\nCUSTOMER\n{d.Customer}\n\nPICKUP\n{d.Pickup}\n\nDROP-OFF\n{d.Dropoff}\n\nDRIVER\n{d.Driver}\n\nSTATUS\n{d.Status}\n\nFARE  R{d.Fare:0}     TIP  R{d.Tip:0}" };
        var assign=DialogButton("Assign Driver",0,390,Gold,Ink); var change=DialogButton("Change Status",170,390,Navy,Color.White);
        assign.Click += (_,__)=>{AssignDriver(d);f.Close();}; change.Click += (_,__)=>{ChangeStatus(d);f.Close();}; left.Controls.Add(change); left.Controls.Add(assign); left.Controls.Add(info);
        var right = new Panel { Dock=DockStyle.Fill, Padding=new Padding(24), BackColor=Bg };
        var title=new Label{Text="Trip Chat",Dock=DockStyle.Top,Height=42,ForeColor=Navy,Font=new Font("Segoe UI",14,FontStyle.Bold)};
        var chat=new ListBox{Dock=DockStyle.Fill,Font=new Font("Segoe UI",10),BorderStyle=BorderStyle.FixedSingle}; chat.Items.Add("Customer: Please call when you are close."); chat.Items.Add("Driver: I am on the way.");
        var compose=new Panel{Dock=DockStyle.Bottom,Height=58}; var box=new TextBox{Left=0,Top=10,Width=350,Height=32,Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top}; var send=DialogButton("Send",360,8,Gold,Ink); send.Width=90; send.Anchor=AnchorStyles.Top|AnchorStyles.Right; send.Click += (_,__)=>{if(!string.IsNullOrWhiteSpace(box.Text)){chat.Items.Add("Control: "+box.Text.Trim());box.Clear();chat.TopIndex=chat.Items.Count-1;}}; compose.Controls.Add(box);compose.Controls.Add(send);
        right.Controls.Add(chat);right.Controls.Add(compose);right.Controls.Add(title);f.Controls.Add(right);f.Controls.Add(left);f.ShowDialog(this);
    }

    void ShowDrivers()
    {
        var root=PagePanel(); var title=new Label{Text="Driver Fleet",Dock=DockStyle.Top,Height=44,ForeColor=Navy,Font=new Font("Segoe UI",13,FontStyle.Bold)}; var g=BaseGrid(); g.Dock=DockStyle.Fill;
        foreach(var h in new[]{"Driver","Phone","Vehicle","Status","Active Jobs","Rating"})g.Columns.Add(h,h);
        foreach(var d in drivers)g.Rows.Add(d.Name,d.Phone,d.Vehicle,d.Status,d.ActiveJobs,d.Rating.ToString("0.0")+" ★");
        root.Controls.Add(g);root.Controls.Add(title);SetPage("Drivers",root);
    }

    void ShowCustomers()
    {
        var root=PagePanel();var g=BaseGrid();g.Dock=DockStyle.Fill;foreach(var h in new[]{"Customer","Orders","Total Spend","Tips","Last Status"})g.Columns.Add(h,h);
        foreach(var grp in deliveries.GroupBy(d=>d.Customer))g.Rows.Add(grp.Key,grp.Count(),"R"+grp.Sum(x=>x.Fare+x.Tip).ToString("0"),"R"+grp.Sum(x=>x.Tip).ToString("0"),grp.First().Status);
        root.Controls.Add(g);SetPage("Customers",root);
    }

    void ShowFinance()
    {
        var root=PagePanel();var stats=new FlowLayoutPanel{Dock=DockStyle.Top,Height=132,BackColor=Bg,WrapContents=false};
        stats.Controls.Add(StatCard("Delivery Fees","R"+deliveries.Sum(x=>x.Fare).ToString("0"),"Gross test revenue"));stats.Controls.Add(StatCard("Driver Tips","R"+deliveries.Sum(x=>x.Tip).ToString("0"),"100% driver tips"));stats.Controls.Add(StatCard("Orders",deliveries.Count.ToString(),"All test orders"));
        var g=BaseGrid();g.Dock=DockStyle.Fill;foreach(var h in new[]{"Order","Customer","Payment","Fare","Tip","Total"})g.Columns.Add(h,h);foreach(var d in deliveries)g.Rows.Add(d.Id,d.Customer,d.Payment,"R"+d.Fare.ToString("0"),"R"+d.Tip.ToString("0"),"R"+(d.Fare+d.Tip).ToString("0"));
        root.Controls.Add(g);root.Controls.Add(stats);SetPage("Finance",root);
    }

    void ShowSettings()
    {
        var root=PagePanel();var box=new Panel{Dock=DockStyle.Top,Height=250,BackColor=Color.White,Padding=new Padding(22)};
        box.Controls.Add(new Label{Text="DROPLINK Management Settings",AutoSize=true,ForeColor=Navy,Font=new Font("Segoe UI",14,FontStyle.Bold),Location=new Point(22,20)});
        box.Controls.Add(new Label{Text="This is the local test control application.\n\nNext production connection:\n• Customer orders sync to Control\n• Control assigns jobs to Driver app\n• Live GPS and trip status sync\n• Real trip chat and push notifications\n• Online payments and reporting",AutoSize=true,ForeColor=Ink,Location=new Point(22,62)});
        root.Controls.Add(box);SetPage("Settings",root);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventsWinForm0302
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            button1.Enabled = false;
        }
        // dışarıdan gelenin atamasını yapabilmek için field tanımladık
        DataRowView _gelen;


        public Form2(DataRowView gelen, string islem)
        {
            InitializeComponent();
            _gelen = gelen;
            if (islem == "Guncelle")
            {
                button1.Enabled = false;
                button2.Enabled = true;
            }
            else if (islem == "Sil")
            {
                button1.Enabled = true;
                button2.Enabled = false;
            }
        }
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["EventsWinForm0302.Properties.Settings.NorthwindConnectionString"].ConnectionString);

        public delegate void UrunEkleEventHandler();
        public event UrunEkleEventHandler UrunEkle;
        // event koyduğumuz da bu tetiklenince event'i kendi üstüne alıyor, event eklemeye gerek kalmıyor

        public delegate void UrunGuncelleEventHandler();
        public event UrunGuncelleEventHandler UrunGuncelle;

        public delegate void UrunSilEventHandler();
        public event UrunSilEventHandler UrunSil;


        private void Form2_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'northwindDataSet2.Categories' table. You can move, or remove it, as needed.
            this.categoriesTableAdapter.Fill(this.northwindDataSet2.Categories);

            if (_gelen != null)
            {
                // update
                textBox1.Text = _gelen["ProductName"].ToString();
                textBox2.Text = _gelen["UnitPrice"].ToString();
                textBox3.Text = _gelen["UnitsInStock"].ToString();
                textBox4.Text = _gelen["UnitsOnOrder"].ToString();

                comboBox1.SelectedValue = _gelen["CategoryID"];
            }
            else
            {
                MessageBox.Show("Lütfen yönergelere göre kayıt giriniz!!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_gelen != null)
            {
                // güncelle
                string sorguGuncelle = "Update Products SET " +
                    "ProductName = @name, UnitPrice = @price, UnitsInStock = @stock, UnitsOnOrder = @order,CategoryID = @catId Where ProductID = @id";
                
                using (SqlCommand cmd = new SqlCommand(sorguGuncelle, conn))
                {
                    cmd.Parameters.AddWithValue("@name", textBox1.Text);
                    cmd.Parameters.AddWithValue("@price", textBox2.Text);
                    cmd.Parameters.AddWithValue("@stock", textBox3.Text);
                    cmd.Parameters.AddWithValue("@order", textBox4.Text);
                    cmd.Parameters.AddWithValue("@catId", comboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@id", _gelen["ProductID"]);

                    if(conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        if(UrunGuncelle != null)
                        {
                            UrunGuncelle();
                            MessageBox.Show("Kayıt başarıyla güncellendi");
                        }
                        this.Close();
                    }
                    else
                    {
                        conn.Close();
                    }       
                }                
            }
            else
            {
                // ekle
                string sorguEkle = "Insert Products (ProductName, UnitPrice, UnitsInStock, UnitsOnOrder, CategoryID) " +
                    "Values (@name, @price, @stock, @order, @catId)";
                using (SqlCommand cmd = new SqlCommand(sorguEkle, conn))
                {
                    cmd.Parameters.AddWithValue("@name", textBox1.Text);
                    cmd.Parameters.AddWithValue("@price", textBox2.Text);
                    cmd.Parameters.AddWithValue("@stock", textBox3.Text);
                    cmd.Parameters.AddWithValue("@order", textBox4.Text);
                    cmd.Parameters.AddWithValue("@catId", comboBox1.SelectedValue);

                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        if (UrunEkle != null)
                        {
                            UrunEkle();
                            MessageBox.Show("Kayıt başarıyla oluşturuldu");
                        }                      
                        this.Close();
                    }
                    else
                    {
                        conn.Close();
                    }
                      
                }                   
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string sorguDelete = "Delete from Products where ProductID = @id";

            if(_gelen != null)
            {
                using (SqlCommand cmd = new SqlCommand(sorguDelete, conn))
                {
                    cmd.Parameters.AddWithValue("@id", _gelen["ProductID"]);
                    try
                    {
                        DialogResult res = MessageBox.Show("Ciddi misin oğlum", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        if (res == DialogResult.OK)
                        {
                            if (conn.State == ConnectionState.Closed)
                            {

                                conn.Open();
                                cmd.ExecuteNonQuery();

                                if (UrunSil != null)
                                {
                                    UrunSil();
                                    MessageBox.Show("Ürün başarıyla silindi");
                                }
                                this.Close();

                            }
                            else
                            {
                                conn.Close();
                            }
                        }
                        if (res == DialogResult.Cancel)
                        {
                            Form1 f1 = new Form1();
                            f1.Show();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Beklenmedik bir hatayla karşılaşıldı!!");
                    }

                }
            }
            else
            {
                MessageBox.Show("Lütfen silinecek kayıt seçiniz!!");
            }

        }
    }
}

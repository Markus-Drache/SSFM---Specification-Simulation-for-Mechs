using System.Data;
using System.Windows.Forms;
using DB_Conektion;

namespace SSfM_UI
{
    public partial class Form1 : Form
    {
        private Conect db;
        private Button selectedButton = null;

        public Form1()
        {
            InitializeComponent();
            db = new Conect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void LoadDataGrid()
        {
            if (selectedButton == null) return;

            var conect = new DB_Conektion.Conect();
            DataTable dt = conect.GetAvailableEquipment(); // example table
            dataGridView1.DataSource = dt;

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
        }

        private void dgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (selectedButton == null || e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            string name = row.Cells["type"].Value.ToString(); // column from your DB
            selectedButton.Text = name;
        }





        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetFractions();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("Fractions table is empty.");
                
               
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetTechnologyNiveaus();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("TechnologyNiveaus table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetTechnologyAvailabilityen();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("TechnologyAvailabilityen table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }


        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetAvailableInternealStruktur();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("InternealStruktur table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }


        private void Button5_Click(object sender, EventArgs e)
        {
            //This needs to be a number field for tonnage instead of button
            //Will change once I have the actual DB 
            int tonnage = 0;

            try
            {
                
                DataTable table = db.GetAvailableSlots(tonnage);

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("Slots table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }


        private void Button6_Click(object sender, EventArgs e)
        {
            //Also dependant on tonnage 
            int tonnage = 0;
            try
            {
                DataTable table = db.GetAvailableInternalStructureIntegrity(tonnage);

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("InternalStructureIntegrity table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetAvailableMusculature();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("Musculature table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetAvailableActivator();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("Activator table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }


        private void Button9_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetAvailableGyroscope();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("Gyroscope table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }



        private void Button10_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = db.GetAvailableReactor();

                dataGridView1.DataSource = table;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                if (table.Rows.Count == 0)
                    MessageBox.Show("Reactor table is empty.");



            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }






        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}


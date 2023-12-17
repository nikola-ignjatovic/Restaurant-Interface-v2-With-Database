using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TvpProjekat2
{
    public partial class Form1 : Form
    {
        // Za animaciju
        private Label threadingLabel;
        private Thread animationThread;
        private bool isMovingRight = true;
        private const int AnimationSpeed = 1; // Adjust this value to control animation speed
        private const int AnimationDistance = 1; // Adjust this value to control animation distance
        private bool shouldTimerExecute = true;

        private List<Label> listOfLabels = new List<Label>();

        int idAktivnogRacuna;


        RestoranDataSet ds;
        RestoranDataSetTableAdapters.JeloTableAdapter dataJelo;
        RestoranDataSetTableAdapters.PrilogTableAdapter dataPrilog;
        RestoranDataSetTableAdapters.PripadnostTableAdapter dataPripadnost;
        RestoranDataSetTableAdapters.Stavka_racunaTableAdapter dataStavka;
        RestoranDataSetTableAdapters.RacunTableAdapter dataRacun;

        bool prikazaniSviRacuni = true;

        List<RadioButton> listOfRadioButtons = new List<RadioButton>();

        int jeloCena, prilogCena;
        string jeloIme = null, prilogIme = null;
        int id_jelo, id_prilog;


        DataTable racun = new DataTable();
        public Form1()
        {
            InitializeComponent();

            threadingLabel = label1;

            animationThread = new Thread(AnimateLabel);



            ds = new RestoranDataSet();
            dataJelo = new RestoranDataSetTableAdapters.JeloTableAdapter();
            dataPrilog = new RestoranDataSetTableAdapters.PrilogTableAdapter();
            dataPripadnost = new RestoranDataSetTableAdapters.PripadnostTableAdapter();
            dataStavka = new RestoranDataSetTableAdapters.Stavka_racunaTableAdapter();
            dataRacun = new RestoranDataSetTableAdapters.RacunTableAdapter();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var rezultat = from cena
                           in ds.Jelo
                           orderby cena.cena
                           select cena;

            // dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = rezultat.ToList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var rezultat = from cena
                           in ds.Jelo
                           orderby cena.cena descending
                           select cena;
            // dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = rezultat.ToList();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var rezultat = from jeloRed
                           in ds.Jelo
                           where jeloRed.naziv.ToLower() == textBox1.Text.ToLower()
                           select jeloRed;

            // dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = rezultat.ToList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Nema porucenih jela";
            idAktivnogRacuna = 0;

            dataJelo.Fill(ds.Jelo);
            dataPripadnost.Fill(ds.Pripadnost);
            dataPrilog.Fill(ds.Prilog);
            dataRacun.Fill(ds.Racun);
            dataStavka.Fill(ds.Stavka_racuna);

            upisiDeafultStvar();

            animationThread.Start();
        }

        private void izracunajNajprodavanijeJelo()
        {
            var rezultat = (from stavkaRacuna
                           in ds.Stavka_racuna
                            group stavkaRacuna by stavkaRacuna.Field<int>("id_jelo") into grouped
                            select new
                            {
                                jeloId = grouped.Key,
                                TotalQuantity = grouped.Sum(item => item.Field<int>("kolicinaPorudzbina"))
                            }).OrderByDescending(group => group.TotalQuantity)
                                .FirstOrDefault();
            if (rezultat == null)
                return;


            var jeloRezultat = from jelo
                               in ds.Jelo
                               where jelo.id_jelo == rezultat.jeloId
                               select jelo;

            label1.Text = jeloRezultat.ToArray()[0].naziv;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            RefreshujGrid();
        }

        private void upisiDeafultStvar()
        {
            if (ds != null)
            {
                if (ds.Prilog.Rows.Count == 0)
                {
                    RestoranDataSet.PrilogRow newPrilogRow = ds.Prilog.NewPrilogRow();
                    newPrilogRow.id_prilog = 1;
                    newPrilogRow.naziv = "kecap";
                    newPrilogRow.cena = 300;
                    ds.Prilog.Rows.Add(newPrilogRow);
                    dataPrilog.Update(ds.Prilog); // Update the database
                    ds.AcceptChanges();


                    RestoranDataSet.PrilogRow newPrilogRow2 = ds.Prilog.NewPrilogRow();
                    newPrilogRow2.id_prilog = 2;
                    newPrilogRow2.naziv = "Majonez";
                    newPrilogRow2.cena = 200;
                    ds.Prilog.Rows.Add(newPrilogRow2);
                    dataPrilog.Update(ds.Prilog); // Update the database
                    ds.AcceptChanges();

                    RestoranDataSet.PrilogRow newPrilogRow3 = ds.Prilog.NewPrilogRow();
                    newPrilogRow3.id_prilog = 0;
                    newPrilogRow3.naziv = "Bez priloga";
                    newPrilogRow3.cena = 0;
                    ds.Prilog.Rows.Add(newPrilogRow3);
                    dataPrilog.Update(ds.Prilog); // Update the database
                    ds.AcceptChanges();


                }
                if (ds.Jelo.Rows.Count == 0)
                {
                    RestoranDataSet.JeloRow newJeloRow = ds.Jelo.NewJeloRow();
                    newJeloRow.id_jelo = 1;
                    newJeloRow.naziv = "Spagete";
                    newJeloRow.cena = 300;
                    ds.Jelo.Rows.Add(newJeloRow);
                    dataJelo.Update(ds.Jelo); // Update the database
                    ds.AcceptChanges();

                    RestoranDataSet.JeloRow newJeloRow2 = ds.Jelo.NewJeloRow();
                    newJeloRow2.id_jelo = 2;
                    newJeloRow2.naziv = "Pica";
                    newJeloRow2.cena = 400;
                    ds.Jelo.Rows.Add(newJeloRow2);
                    dataJelo.Update(ds.Jelo); // Update the database
                    ds.AcceptChanges();

                    RestoranDataSet.JeloRow newJeloRow3 = ds.Jelo.NewJeloRow();
                    newJeloRow3.id_jelo = 3;
                    newJeloRow3.naziv = "KAVIJAR";
                    newJeloRow3.cena = 700;
                    ds.Jelo.Rows.Add(newJeloRow3);
                    dataJelo.Update(ds.Jelo); // Update the database
                    ds.AcceptChanges();

                    RefreshujGrid();
                }

                if (ds.Pripadnost.Rows.Count == 0)
                {

                    RestoranDataSet.PripadnostRow newPripadnostRow0 = ds.Pripadnost.NewPripadnostRow();
                    newPripadnostRow0.id_jelo = 1;
                    newPripadnostRow0.id_prilog = 0;
                    ds.Pripadnost.Rows.Add(newPripadnostRow0);
                    // dataPripadnost.Update(ds.Pripadnost); // Update the database
                    //  ds.AcceptChanges();

                    RestoranDataSet.PripadnostRow newPripadnostRow = ds.Pripadnost.NewPripadnostRow();
                    newPripadnostRow.id_jelo = 1;
                    newPripadnostRow.id_prilog = 1;
                    ds.Pripadnost.Rows.Add(newPripadnostRow);
                    // dataPripadnost.Update(ds.Pripadnost); // Update the database
                    //  ds.AcceptChanges();

                    RestoranDataSet.PripadnostRow newPripadnostRow2 = ds.Pripadnost.NewPripadnostRow();
                    newPripadnostRow2.id_jelo = 1;
                    newPripadnostRow2.id_prilog = 2;
                    ds.Pripadnost.Rows.Add(newPripadnostRow2);
                    // dataPripadnost.Update(ds.Pripadnost); // Update the database
                    // ds.AcceptChanges();



                    RestoranDataSet.PripadnostRow newPripadnostRow10 = ds.Pripadnost.NewPripadnostRow();
                    newPripadnostRow10.id_jelo = 2;
                    newPripadnostRow10.id_prilog = 0;
                    ds.Pripadnost.Rows.Add(newPripadnostRow10);

                    RestoranDataSet.PripadnostRow newPripadnostRow3 = ds.Pripadnost.NewPripadnostRow();
                    newPripadnostRow3.id_jelo = 2;
                    newPripadnostRow3.id_prilog = 1;
                    ds.Pripadnost.Rows.Add(newPripadnostRow3);
                    //dataPripadnost.Update(ds.Pripadnost); // Update the database
                    //  ds.AcceptChanges();

                    RestoranDataSet.PripadnostRow newPripadnostRow4 = ds.Pripadnost.NewPripadnostRow();
                    newPripadnostRow4.id_jelo = 2;
                    newPripadnostRow4.id_prilog = 2;
                    ds.Pripadnost.Rows.Add(newPripadnostRow4);
                    // dataPripadnost.Update(ds.Pripadnost); // Update the database
                    // ds.AcceptChanges();

                    RestoranDataSet.PripadnostRow newPripadnostRow5 = ds.Pripadnost.NewPripadnostRow();
                    newPripadnostRow5.id_jelo = 3;
                    newPripadnostRow5.id_prilog = 0;
                    ds.Pripadnost.Rows.Add(newPripadnostRow5);
                    // dataPripadnost.Update(ds.Pripadnost); // Update the database
                    // ds.AcceptChanges();
                }

                if (ds.Racun.Rows.Count == 0)
                {
                    RestoranDataSet.RacunRow newRacunRow = ds.Racun.NewRacunRow();
                    newRacunRow.id_racun = 0;
                    newRacunRow.ukupna_cena = 0;
                    newRacunRow.datum = DateTime.Now;


                    ds.Racun.Rows.Add(newRacunRow);
                    dataRacun.Update(ds.Racun); // Update the database
                    ds.AcceptChanges();

                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string imeJela = textBoxJelo.Text;
            string ideviPriloga = textBoxPrilozi.Text;
            string cenaJela = textBoxCena.Text;

            if (string.IsNullOrWhiteSpace(imeJela))
            {
                MessageBox.Show("Niste lepo uneli ime jela");
                return;
            }
            var rezultat = from jelo
                        in ds.Jelo
                           where imeJela.ToLower() == jelo.naziv.ToLower()
                           select jelo;

            if (rezultat.Any())
            {
                MessageBox.Show("Jelo vec postoji");
                return;
            }

            if (string.IsNullOrWhiteSpace(ideviPriloga))
            {
                MessageBox.Show("Polje za priloge ne moze biti prazno unesite 0 za prazan prilog");
                return;
            }

            if (!ContainsOnlyNumbersAndWhitespaces(ideviPriloga))
            {
                MessageBox.Show("Ne mozes karaktere dobar pokusaj Tamara");
                return;
            }

            // Extracting numbers from ideevi priloga string to process them further
            int[] ideviPrilogaNiz = ExtractNumbers(ideviPriloga);

            for (int i = 0; i < ideviPrilogaNiz.Length; i++)
            {
                var rezultat2 = from prilog
                                        in ds.Prilog
                                where ideviPrilogaNiz[i] == prilog.id_prilog
                                select prilog;
                if (!rezultat2.Any())
                {
                    MessageBox.Show("Prilog sa brojem: " + ideviPrilogaNiz[i].ToString() + " ne postoji");
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(cenaJela))
            {
                MessageBox.Show("Unesite cenu jela");
                return;
            }

            if (!Int32.TryParse(cenaJela, out int result))
            {
                MessageBox.Show("Aj broj unesi molim te");
                return;
            }

            int cenaJelaInt = int.Parse(cenaJela);

            if (cenaJelaInt < 0)
            {
                MessageBox.Show("CENA NE MOZE DA BUDE MANJA OD 0");
                return;
            }

            // Sad samo jelo treba da se unese u tabelu i ostale tabele

            RestoranDataSet.JeloRow novoJelo = ds.Jelo.NewJeloRow();
            novoJelo.naziv = imeJela;
            novoJelo.cena = cenaJelaInt;
            ds.Jelo.Rows.Add(novoJelo);
            dataJelo.Update(ds.Jelo); // Update the database
            ds.AcceptChanges();

            for (int i = 0; i < ideviPrilogaNiz.Length; i++)
            {
                RestoranDataSet.PripadnostRow novaPripadnost = ds.Pripadnost.NewPripadnostRow();
                novaPripadnost.id_jelo = novoJelo.id_jelo;
                novaPripadnost.id_prilog = ideviPrilogaNiz[i];
                ds.Pripadnost.Rows.Add(novaPripadnost);
                //dataPripadnost.Update(ds.Pripadnost); // Update the database
                ds.AcceptChanges();
            }

            MessageBox.Show("Jelo uspesno dodato");

            textBoxJelo.Clear();
            textBoxPrilozi.Clear();
            textBoxCena.Clear();
            RefreshujGrid();
        }

        static bool ContainsOnlyNumbersAndWhitespaces(string input)
        {
            // Regular expression pattern to match numbers and white spaces
            string pattern = @"^[0-9\s]+$";

            return Regex.IsMatch(input, pattern);
        }

        static int[] ExtractNumbers(string input)
        {
            // Regular expression pattern to match numbers
            string pattern = @"\d+";

            var matches = Regex.Matches(input, pattern);

            return matches
                .Cast<Match>()
                .Select(match => int.Parse(match.Value))
                .ToArray();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool pronasaoPrilog = false;
            DataGridViewRow userClickRow = dataGridView1.CurrentRow;
            if (userClickRow == null)
            {
                MessageBox.Show("Odaberite jelo koje zelite da porucite");
                return;
            }
            int idJela = Convert.ToInt32(userClickRow.Cells[0].Value);
            int idPriloga = 0;

            for (int i = 0; i < listOfRadioButtons.Count(); i++)
            {
                if (listOfRadioButtons[i].Checked)
                {
                    idPriloga = Convert.ToInt32(listOfRadioButtons[i].Name);
                    pronasaoPrilog = true;
                    break;
                }
            }

            if (pronasaoPrilog == false)
            {
                MessageBox.Show("Molim vas izaberite prilog nisam placen da radim ovo");
                return;
            }

            var rezultat = from idJelo
                               in ds.Jelo
                           where idJelo.id_jelo == idJela
                           select idJelo.cena;
            int cenaJela = rezultat.ToArray()[0];

            var rezultat2 = from idPrilog
                              in ds.Prilog
                            where idPrilog.id_prilog == idPriloga
                            select idPrilog.cena;
            int cenaPriloga = rezultat2.ToArray()[0];



            var rezultatX = from jelo
                            in ds.Stavka_racuna
                            where jelo.id_jelo == idJela && jelo.id_prilog == idPriloga
                            select jelo;


            if (rezultatX.Any())
            {
                rezultatX.ToArray()[0].kolicinaPorudzbina++;

            }
            else
            {

                RestoranDataSet.Stavka_racunaRow newStavkaRacuna = ds.Stavka_racuna.NewStavka_racunaRow();
                newStavkaRacuna.id_racun = idAktivnogRacuna;
                newStavkaRacuna.id_prilog = idPriloga;
                newStavkaRacuna.id_jelo = idJela;
                newStavkaRacuna.cenaJelo = cenaJela;
                newStavkaRacuna.cenaPrilog = cenaPriloga;
                newStavkaRacuna.kolicinaPorudzbina = 1;


                ds.Stavka_racuna.Rows.Add(newStavkaRacuna);
                // dataStavka.Update(ds.Stavka_racuna); // Update the database
                // ds.AcceptChanges();
            }
            // Treba ukupnu cenu racuna da promenim

            var rezultat3 = from racun
                              in ds.Racun
                            where racun.id_racun == idAktivnogRacuna
                            select racun;
            if (!rezultat.Any())
            {
                return;
            }
            rezultat3.ToArray()[0].ukupna_cena = rezultat3.ToArray()[0].ukupna_cena + cenaPriloga + cenaJela;
            //dataRacun.Update(ds.Racun); // Update the database
            //ds.AcceptChanges();
            RefreshujGrid();

        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            for (int i = 0; i < listOfRadioButtons.Count(); i++)
            {
                this.Controls.Remove(listOfRadioButtons[i]);
                listOfRadioButtons[i].Dispose(); // Dispose the control to release resources
            }
            listOfRadioButtons.Clear();
            if (e.RowIndex >= 0)  // Make sure a valid row was clicked (not header or empty space)
            {
                DataGridViewRow userClickRow = dataGridView1.Rows[e.RowIndex];

                int idJela = Convert.ToInt32(userClickRow.Cells[0].Value);

                var rezultat = from idJelo
                               in ds.Pripadnost
                               where idJelo.id_jelo == idJela
                               select idJelo.id_prilog;

                var arrayOfPrilogs = rezultat.ToArray();

                for (int i = 0; i < arrayOfPrilogs.Length; i++)
                {
                    var rezultat2 = from idPrilog
                                    in ds.Prilog
                                    where arrayOfPrilogs[i] == idPrilog.id_prilog
                                    select idPrilog;

                    var arrayOfPrilog = rezultat2.ToArray();

                    RadioButton radioButton = new RadioButton();
                    radioButton.Text = arrayOfPrilog[0].naziv + " Cena: " + arrayOfPrilog[0].cena.ToString();
                    radioButton.Name = arrayOfPrilog[0].id_prilog.ToString();
                    radioButton.Location = new System.Drawing.Point(400, 260 + 30 * i);
                    radioButton.Size = new System.Drawing.Size(500, 30);

                    // Attach event handler for CheckedChanged event
                    radioButton.CheckedChanged += RadioButton_CheckedChanged;

                    listOfRadioButtons.Add(radioButton);

                    // Add the RadioButton to the form's Controls collection
                    this.Controls.Add(radioButton);


                    var rezultat6 = from stavkaRacuna
                      in ds.Stavka_racuna
                                    select
                                        ds.Stavka_racuna.Sum(item => item.Field<int>("kolicinaPorudzbina"));

                    if (rezultat6.Any())
                    {
                        var kolikoPutaSeToJeloProdalo = ds.Stavka_racuna
      .Where(stavkaRacuna => stavkaRacuna.id_jelo == idJela)
      .Sum(item => item.Field<int>("kolicinaPorudzbina"));

                        int[] data = { rezultat6.ToArray()[0] - kolikoPutaSeToJeloProdalo, kolikoPutaSeToJeloProdalo };
                        paint(data);

                    }
                }


            }
        }

        private void paint(int[] data)
        {
            foreach (Label x in listOfLabels)
            {
                Controls.Remove(x);
            }
            // Create a label

            int ukupno = data[1] + data[0];

            float procenatOstalih = (data[0] * 1f / ukupno * 1f) * 100f;
            float procenatSelektovanih = (data[1] * 1f / ukupno * 1f) * 100f;

            Label label = new Label();
            label.Text = "Zelena predstavlja odabrano jelo kolicina: " + data[1].ToString() + " Procenat: " + procenatSelektovanih.ToString() + "%";
            label.Location = new System.Drawing.Point(450, 450);
            label.Size = new System.Drawing.Size(400, 20);

            // Add the label to the form's Controls collection
            this.Controls.Add(label);

            // Create a label
            Label label2 = new Label();
            label2.Text = "Crvena predstavlja ostala jela kolicina: " + data[0].ToString() + " Procenat: " + procenatOstalih.ToString() + "%";
            label2.Location = new System.Drawing.Point(450, 470);
            label2.Size = new System.Drawing.Size(400, 20);

            // Add the label to the form's Controls collection
            this.Controls.Add(label2);

            listOfLabels.Add(label);
            listOfLabels.Add(label2);

            Color[] colors = { Color.Red, Color.Green };
            Refresh(); // Clear previous drawing

            int total = 0;
            foreach (int value in data)
            {
                total += value;
            }

            int startAngle = 0;
            for (int i = 0; i < data.Length; i++)
            {
                int sweepAngle = (int)((data[i] / (float)total) * 360);
                using (Brush brush = new SolidBrush(colors[i]))
                {
                    CreateGraphics().FillPie(brush, 450, 500, 200, 200, startAngle, sweepAngle);
                }
                startAngle += sweepAngle;
            }
        }


        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                MessageBox.Show(radioButton.Text + " selected!");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            RefreshujGrid();



            RestoranDataSet.RacunRow newRacunRow = ds.Racun.NewRacunRow();
            newRacunRow.ukupna_cena = 0;
            newRacunRow.datum = DateTime.Now;

            ds.Racun.Rows.Add(newRacunRow);
            //dataRacun.Update(ds.Racun); // Update the database
            ds.AcceptChanges();

            dataGridView2.DataSource = "";
            listBox1.Items.Clear();

            idAktivnogRacuna = newRacunRow.id_racun;

            MessageBox.Show("Uspesno ste napravili novi racun");
        }

        private void dataGridView2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            listBox1.Items.Clear();
            if (e.RowIndex < 0)
                return;
            DataGridViewRow userClickRow = dataGridView2.Rows[e.RowIndex];

            int idRacuna = Convert.ToInt32(userClickRow.Cells[0].Value);

            var rezultat = from stavkaRacuna
                           in ds.Stavka_racuna
                           where stavkaRacuna.id_racun == idRacuna
                           select stavkaRacuna;
            var rezultatArray = rezultat.ToArray();

            for (int i = 0; i < rezultatArray.Length; i++)
            {
                var jeloRezultat = from Jelo
                                in ds.Jelo
                                   where Jelo.id_jelo == rezultatArray[i].id_jelo
                                   select Jelo;

                var jeloSpecificno = jeloRezultat.ToArray()[0];

                var prilogRezultat = from Prilog
                                     in ds.Prilog
                                     where Prilog.id_prilog == rezultatArray[i].id_prilog
                                     select Prilog;

                var prilogSpecifican = prilogRezultat.ToArray()[0];

                string stavkaString = "Ime Jela: " + jeloSpecificno.naziv + " Cena jela: " + jeloSpecificno.cena + "\n" + "  Ime priloga: " + prilogSpecifican.naziv + " Cena Priloga: " + prilogSpecifican.cena + "  X" + rezultatArray[i].kolicinaPorudzbina + "\n";

                listBox1.Items.Add(stavkaString);

            }
        }

        private async void sviRacuniButton_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (prikazaniSviRacuni == false)
            {
                sviRacuniButton.Text = "Prikazi samo aktivni racun";
                prikazaniSviRacuni = true;
                var rezultat4 = from Racun
                        in ds.Racun
                                select Racun;
                // dataGridView1.AutoGenerateColumns = true;
                dataGridView2.DataSource = rezultat4.ToList();
                shouldTimerExecute = true;
                await Task.Delay(2000);
                sviRacuniButtonClick2(sender, e);
            }
            else
            {
                shouldTimerExecute = false;
                sviRacuniButton.Text = "Prikazi sve racune na 2 sekunde ili sam iskljuci pritiskom na isto dugme";
                var rezultat4 = from Racun
                        in ds.Racun
                                where Racun.id_racun == idAktivnogRacuna
                                select Racun;
                // dataGridView1.AutoGenerateColumns = true;
                dataGridView2.DataSource = rezultat4.ToList();
                prikazaniSviRacuni = false;
            }

        }

        private void sviRacuniButtonClick2(object sender, EventArgs e)
        {
            if (shouldTimerExecute)
            {
                sviRacuniButton.Text = "Prikazi sve racune na 2 sekunde ili sam iskljuci pritiskom na isto dugme";
                var rezultat4 = from Racun
                        in ds.Racun
                                where Racun.id_racun == idAktivnogRacuna
                                select Racun;
                // dataGridView1.AutoGenerateColumns = true;
                dataGridView2.DataSource = rezultat4.ToList();
                prikazaniSviRacuni = false;
            }
        }

        private void RefreshujGrid()
        {
            foreach (Label x in listOfLabels)
            {
                this.Controls.Remove(x);
            }
            Invalidate();
            var rezultat = from cena
                          in ds.Jelo
                           orderby cena.cena
                           select cena;

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = rezultat.ToList();

            for (int i = 0; i < listOfRadioButtons.Count(); i++)
            {
                this.Controls.Remove(listOfRadioButtons[i]);
                listOfRadioButtons[i].Dispose(); // Dispose the control to release resources
            }
            listOfRadioButtons.Clear();

            var rezultat4 = from Racun
                          in ds.Racun
                            where Racun.id_racun == idAktivnogRacuna
                            select Racun;
            // dataGridView1.AutoGenerateColumns = true;
            dataGridView2.DataSource = rezultat4.ToList();
            listBox1.Items.Clear();

            izracunajNajprodavanijeJelo();
        }

        private void AnimateLabel()
        {
            while (true)
            {
                if (isMovingRight)
                {
                    // Move the label to the right
                    if (threadingLabel.Right + AnimationDistance < Width)
                    {
                        threadingLabel.BeginInvoke((MethodInvoker)delegate
                        {
                            threadingLabel.Left += AnimationDistance;
                        });
                    }
                    else
                    {
                        isMovingRight = false;
                    }
                }
                else
                {
                    // Move the label to the left
                    if (threadingLabel.Left - AnimationDistance > 0)
                    {
                        threadingLabel.BeginInvoke((MethodInvoker)delegate
                        {
                            threadingLabel.Left -= AnimationDistance;
                        });
                    }
                    else
                    {
                        isMovingRight = true;
                    }
                }

                Thread.Sleep(AnimationSpeed);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop the animation thread when the form is closing
            animationThread.Abort();
            base.OnFormClosing(e);
        }

    }
}

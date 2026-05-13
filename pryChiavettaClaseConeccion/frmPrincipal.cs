using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pryChiavettaClaseConeccion
{
    public partial class frmPrincipal : Form
    {
        private string currentDbPath = string.Empty;

        public frmPrincipal()
        {
            InitializeComponent();
       
            lblEstado.Text = "Estado: Desconectado";
        }

        // Evento Buscar archivo de base de datos
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            using (var x = new OpenFileDialog())
            {
                x.Filter = "Access Databases|*.accdb;*.mdb|All files|*.*";
                x.Title = "Seleccionar archivo de base de datos Access";
                x.Multiselect = false;

                if (x.ShowDialog() == DialogResult.OK)
                {
                    currentDbPath = x.FileName;
                    txtDbPath.Text = currentDbPath;
                    lblEstado.Text = "Estado: Archivo seleccionado";
                    cmbTablas.Items.Clear();
                    dgvDatos.DataSource = null;
                }
            }
        }

        // Evento: Conectar a la base seleccionada
        private async void btnConectar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentDbPath))
            {
                MessageBox.Show("Por favor seleccione un archivo de base de datos primero.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(currentDbPath))
            {
                MessageBox.Show("El archivo seleccionado no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblEstado.Text = "Estado: Conectando...";
            cmbTablas.Items.Clear();
            dgvDatos.DataSource = null;

            try
            {
                // Obtener tablas en tarea separada
                var tablas = await Task.Run(() => ClaseConeccion.GetTableNames(currentDbPath));

                if (tablas == null || tablas.Count == 0)
                {
                    lblEstado.Text = "Estado: No se encontraron tablas";
                    MessageBox.Show("No se encontraron tablas en la base de datos.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                cmbTablas.Items.AddRange(tablas.ToArray());
                lblEstado.Text = $"Estado: Conectado ({tablas.Count} tablas)";
                MessageBox.Show("Conexión exitosa.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Data.OleDb.OleDbException ex)
            {
                // Errores relacionados con OLEDB (motor no instalado, archivo corrupto, etc.)
                lblEstado.Text = "Estado: Error en conexión";
                MessageBox.Show($"Error al conectar con la base de datos:\n{ex.Message}", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Estado: Error";
                MessageBox.Show($"Ocurrió un error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event Selección de tabla  cargar datos
        private async void cmbTablas_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabla = cmbTablas.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(tabla))
                return;

            lblEstado.Text = $"Estado: Cargando datos de {tabla}...";

            try
            {
                
                var dt = await Task.Run(() => ClaseConeccion.GetTableData(currentDbPath, tabla));
                dgvDatos.DataSource = dt;
                lblEstado.Text = $"Estado: Mostrando {dt.Rows.Count} registros de {tabla}";//carga tbl selc
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Estado: Error al cargar datos";
                MessageBox.Show($"Error al cargar datos de la tabla {tabla}:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {

        }
    }
}

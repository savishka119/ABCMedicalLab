"use strict";
let groupTable;

$(function () {
  groupTable = app.module.controls.load();
});

function Delete(url) {
  app.module.controls.delete(url);
}

let app = {
  module: {
    controls: {
      load: function () {
        return $("#tblData").DataTable({
          ajax: {
            url: "/Main/UserGroups/GetAll",
          },
          scrollX: true,
          columns: [
            { data: "name", autoWidth: true },
            { data: "description", autoWidth: true },
            {
              data: "id",
              render: function (data, type, row) {
                $("#action .btn-success").attr(
                  "href",
                  `/Main/UserGroups/Upsert/${data}`
                );
                $("#action .btn-danger").attr(
                  "onclick",
                  `Delete('/Main/UserGroups/Delete/${data}')`
                );
                return $("#action").clone().html();
              },
              autoWidth: true,
            },
          ],
        });
      },
      delete: function (url) {
        Swal.fire({
          title: "Are you sure you want to Delete?",
          text: "You will not be able to restore the data!",
          icon: "warning",
          showCancelButton: true,
          confirmButtonColor: "#3085d6",
          cancelButtonColor: "#d33",
          confirmButtonText: "Yes, delete it!",
        }).then((willDelete) => {
          if (willDelete.isConfirmed) {
            $.ajax({
              type: "DELETE",
              url: url,
              success: function (data) {
                if (data.success) {
                  toastr.success(data.message);
                  groupTable.ajax.reload();
                } else {
                  toastr.error(data.message);
                }
              },
            });
          }
        });
      },
    },
  },
};

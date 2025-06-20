using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensyu_E_PAN.Models.Acounts
{
    public class User
    {
        [Key]
        public int Id { get; set; }//プライマリ

        [Required]
        [StringLength(50)]
        public string Login_Id {  get; set; }//ログインID

        [Required]
        [StringLength(100)]
        public string Name { get; set; }//名前

        public int Roll_CD { get; set; }//Roll_ListのIDと紐づけ

        [ForeignKey("Roll_CD")]
        public Roll Roll { get; set; }//リレーション用　Roll_Lists


        [Required]
        [StringLength(100)]
        public string Password { get; set; }//パスワード
        [Range(0,int.MaxValue)]
        public int TimePrice_D { get; set; }//日中の時給

        [Range(0, int.MaxValue)]
        public int TimePrice_N { get; set; }//夜間の時給
    }
}

/* --- script.js DOSYASININ İÇİ --- */

let sepet = [];

// Sepete Ekleme Fonksiyonu
function sepeteEkle(urunAdi, fiyat) {
    sepet.push({ isim: urunAdi, fiyat: fiyat });
    document.getElementById("sepet-sayisi").innerText = sepet.length;
    alert(urunAdi + " sepete eklendi!");
}

// Sepeti Açma Fonksiyonu
function sepetiAc() {
    const modal = document.getElementById("sepet-modal");
    const liste = document.getElementById("sepet-listesi");
    const toplamTutar = document.getElementById("toplam-fiyat");
    
    modal.style.display = "block"; 
    liste.innerHTML = ""; 
    
    let toplam = 0;

    sepet.forEach((urun, index) => {
        let satir = document.createElement("li");
        // Silme butonu ile birlikte ürünü listele
        satir.innerHTML = `${urun.isim} - <b>${urun.fiyat} AZN</b> <button onclick="urunuSil(${index})" style="background:red;color:white;border:none;border-radius:3px;cursor:pointer;margin-left:10px;">Sil</button>`;
        liste.appendChild(satir);
        toplam += urun.fiyat;
    });

    if (sepet.length === 0) {
        liste.innerHTML = "<li>Sepetiniz henüz boş.</li>";
    }

    toplamTutar.innerText = "Toplam: " + toplam + " AZN";
}

// Ürün Silme Fonksiyonu
function urunuSil(index) {
    sepet.splice(index, 1);
    sepetiAc(); // Listeyi güncelle
    document.getElementById("sepet-sayisi").innerText = sepet.length;
}

// Sepeti Kapatma Fonksiyonu
function sepetiKapat() {
    document.getElementById("sepet-modal").style.display = "none";
}

// WhatsApp Sipariş Fonksiyonu
function whatsappSiparisVer() {
    if (sepet.length === 0) {
        alert("Sepetiniz boş, lütfen önce ürün ekleyin.");
        return;
    }

    let mesaj = "Merhaba, web sitenizden şu ürünleri sipariş etmek istiyorum:%0A"; 
    let toplam = 0;

    sepet.forEach(urun => {
        mesaj += `- ${urun.isim} (${urun.fiyat} AZN)%0A`;
        toplam += urun.fiyat;
    });

    mesaj += `%0AToplam Tutar: ${toplam} AZN`;

    // TELEFON NUMARAN BURADA
    let telefon = "+994513677405"; 
    
    window.open(`https://wa.me/${telefon}?text=${mesaj}`, "_blank");
}
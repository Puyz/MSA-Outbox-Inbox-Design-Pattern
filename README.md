# Outbox Design Pattern Nedir?

- **Outbox pattern, Guaranteed Delivery Pattern(Garantili Teslimat Deseni)’a dayanır!**

Asenkron iletişim sağlayan sistemlerde, servisler arasında veya servisle message broker arasında gerçekleşen veri iletiminde bağlantının beklenmedik şekilde kopması, hata oluşması veya donanımsal/yazılımsal kesintilerin ortaya çıkması gibi durumlar, veri kaybı riskine ve sistemler arası veri tutarsızlığına yol açabilir. Bu gibi olumsuzlukları önlemek adına, mesajın kaybolmasını engelleyen ve bağlantı yeniden kurulduğunda mesajın hedef noktaya tekrar iletilmesini sağlayan bir yöntem kullanılır. Basit bir yapı gibi görünse de, bu tür yöntemler kritik süreçlerde oldukça yüksek bir etki sağlar.

## Outbox Design Pattern’ın Teorik Davranışı Nasıldır?

![image](https://github.com/user-attachments/assets/5d15149d-d934-4d0d-90eb-1239037377da)


Outbox pattern’ı, özellikle dağıtık sistemlerde veri tutarlılığını sağlamak için kullanılan bir yöntemdir. Teoride, bir servise gelen istekle yapılan işlemler sonucunda başka bir servise veya bir message broker’a mesaj ya da olay (event) gönderilmesi gerektiğinde, Outbox pattern’ı devreye girer. Bu yöntemde, yayınlanacak mesaj önce bir "Outbox" tablosuna kayıt edilir. Bu tablo, mesajın güvenli bir şekilde kayıt altına alınmasını ve hedef servise ya da message broker’a ulaştırılamama durumunda kaybolmamasını sağlar.

Örneğin, bir servisin bir veritabanı işlemi sonrasında başka bir servise mesaj göndermesi gerektiğinde, bu mesaj anında gönderilmeyip önce "Outbox" tablosuna yazılır. Daha sonra bu tabloyu düzenli olarak kontrol eden bir süreç, mesajları sırayla alıp hedefe gönderir. Eğer gönderim sırasında bir hata oluşursa, Outbox tablosunda kayıtlı olan mesaj korunur ve sorun giderildiğinde mesaj yeniden gönderilir. Böylece, veri kaybı önlenmiş ve bütünsel tutarlılık sağlanmış olur.

- **Nihayetinde bakarsanız eğer asenkron süreçte servisler arası iletişim verilerinin Outbox’da tutularak; olası aksaklıklardan, sistem kesintilerinden veya bağlantı kopukluklarından dolayı oluşabilecek iletişim ve veri transferi problemlerinden etkilenmemesi sağlanarak bütünlüğün korunması amaçlanmaktadır.**

## Outbox Pattern Hangi Durumlarda Kullanılır?
Aynı anda iki farklı işleme yönelik kalıcı değişikliklerin yapıldığı durumlarda, Outbox pattern ideal bir çözüm sunar. Örneğin, bir e-ticaret uygulamasında kullanıcıdan gelen sipariş işlemi sırasında hem **`Orders`** tablosuna bir kayıt eklenip hem de siparişin oluşturulduğuna dair **`OrderCreatedEvent`** isimli bir event message broker’a gönderilebilir. Bu tür işlemler, veritabanı ve message broker arasında aynı anda veri yazma işlemi olan *`Dual Write`* olarak adlandırılır.

Dual Write senaryosunda, veritabanına başarılı bir kayıt yapılıp message broker’a gönderimin başarısız olması ya da tam tersi durumlar sistemler arasında veri tutarsızlıklarına neden olabilir. Bu tutarsızlıklar, veritabanı ve eventler arasında senkronize olmamış verilerin ortaya çıkmasına yol açabilir ve sistemin bütünlüğünü bozabilir. Ayrıca, Dual Write kaynaklı tutarsızlıklar sistem sorunsuz çalışırken fark edilmeyebilir ve ancak uzun vadede ortaya çıkarak sorunlara neden olabilir.

Bu tür olası tutarsızlıkları önlemek için Outbox pattern kritik bir işlev üstlenir. Outbox pattern, yapılacak işlemleri bir transaction içinde kaydederek, hem veritabanı işlemimizin tamamlanmasını sağlar hem de gönderilecek event’i güvenli bir şekilde "Outbox" tablosuna ekler. Daha sonra Outbox tablosunda bekleyen event’ler, belirli bir süreç tarafından message broker’a gönderilir. Böylece, veritabanı ve eventlerin her koşulda senkron kalması sağlanarak uygulamanın tutarlılığı korunur.

- Dual Write; distributed, event based vs. gibi uygulamalarda sıklıkla sorunlara neden olan bir durumdur. Ve bu sorunları tespit edebilmek, maliyetleri ölçebilmek ve düzeltebilmek oldukça zor ve zahmetli olacaktır!
- Outbox pattern Dual Write durumu varsa düşünülmelidir!

## Outbox Pattern vs Event Sourcing

Yukarıdaki satırlar sizlere Event Sourcing‘i anımsatmış olabilir. Nihayetinde yapılacak işlemlere dair(publish/subscribe) bir tabloda tutulacak kayıtlar söz konusudur. Evet, Outbox pattern deyince akla event sourcing’in gelmesi normaldir lakin Outbox pattern event sourcing değildir. Belki alternatifidir diyebiliriz ama değildir! Outbox pattern ile event sourcing arasındaki en radikal farkı ele almamız gerekirse eğer; event sourcing’de bir işlenen veriye dair tüm olaylar kayıt altına alınırken, Outbox’da ise işlenecek olan mesajların/event’lerin işlendikten sonra silinmesi ya da işlendiğine dair güncellenmesi söz konusudur. Event sourcing’de kesinlikle herhangi bir olayın silinmesi söz konusu değildir bilakis bir veriye dair yapılan bir işlemin geri alınması bile yine ayrı bir olay olarak kabul edilmekte ve böylece meydana gelen olayların bütünü ilgili verinin nihai halini ortaya sermekte ve süreçte de ne gibi değişimler yaşadığını bizlere söylemektedir.

- Outbox pattern ile ilgili en önemli husus yalnızca geçici bir mesaj/olay deposu olmasıdır. Mesajlar hedef servise ya da message broker’a ulaştırıldığı taktirde ya silinmeli ya da işlendiğine dair güncellenmelidir.

## Outbox Pattern’ın Idempotents Sorunsalı!
Outbox pattern kullanırken, publish edilen mesajların işlendiğini belirlemek veya tablodan silmek, nadiren de olsa oluşabilecek bir handikap içerir. Bu durum, Outbox tablosundaki işaretleme veya silme işlemlerinin, ilgili veritabanıyla olası bir iletişim hatası nedeniyle gerçekleştirilememesi durumunu ifade eder. Mesaj yayınlanmış olabilir ancak Outbox tablosuna bu durum yansıtılamaz. Veritabanı ile tekrar iletişim kurulduğunda, işlenmiş olan mesaj tekrar işleme tabi tutulabilir, bu da uygulama açısından bütünsel tutarlılığı etkileyebilecek bir durumu özetler.

Bu durumla başa çıkmak için, mesajları/event’leri tüketen consumer’ların Idempotent olarak tasarlanması önerilir. Idempotent, matematikte ve bilgisayar bilimlerinde kullanılan, ilk işlem haricinde sonraki tüm işlemler için etkisi olmayan ve sonucu değiştirmeden uygulanabilen bir özellik ifade eder.

Rest mimarisindeki get, put ve delete işlemleri idempotent özellik gösterirken, post işlemi idempotent değildir. İdempotentlik, matematiksel bir terim olarak herhangi bir işlemin tekrarlanması sonucunda ilk işlemin dışında ekstra bir sonuç üretmemesi anlamına gelir. Örneğin, bir kullanıcıyı silme işlemi idempotenttir, çünkü tekrarlanan isteklerde aynı kullanıcı silinmediği için sonuç değişmez. Bunun yanı sıra, post işlemi idempotent özellik göstermez, çünkü her istekte aynı veriyi ekleyerek sonucu etkiler. Günlük hayattan bir örnekle de açıklanan idempotent kavramı, X bankasının bankomatında bakiye sorgulama işlemine benzetilmiştir. Bakiye sorgulama işlemi idempotentken, para çekme veya yatırma işlemi idempotent olmayan bir işlemdir, çünkü her işlem sonrasında bakiye değişir.

- Distributed yapılanmalarda, idempotent davranış genel bir prensiptir, yani bir mesajın birden çok kez yayınlanması aynı etkiyi üretmeli ve güvenlice yeniden gönderilebilmelidir. Bu nedenle, servisler arası iletişimde kullanılacak mesajlar veya event’lerin idempotent olarak tasarlanması, doğru ve tutarlı davranış için kritik bir öneme sahiptir.

### Yayınlanacak message/event’larda idempotency nasıl sağlanır? 
Yayınlanacak mesajlarda idempotency sağlamanın yolu, her bir mesaj veya event için özel bir anahtar oluşturmak ve consumer’ların bu anahtar aracılığıyla daha önce tüketilip tüketilmediğini kontrol etmelerini sağlamaktır. Örneğin, Outbox table’a eklenen verilere IdempotentToken değeri ekleyerek bu kontrolü yapabiliriz. Mesajlar, consumer tarafından önce Inbox Table adlı bir tabloya kaydedilir ve daha sonra işlenir. Eğer aynı IdempotentToken değerine sahip bir kayıt varsa işlem gerçekleştirilmez, aksi halde işleme alınır. Bu yöntem, Inbox Pattern’ını temel alır.

![image](https://github.com/user-attachments/assets/1917a1db-2dc4-4f1e-96aa-86cac2fe584b)

Yukarıdaki görsel şemayı incelerseniz eğer **Outbox table**’a kayıt atılan verilere bir **IdempotentToken** değeri eşlik etmektedir. Bu şekilde publisher ile message broker’a gönderilen mesajlar consumer tarafında işlenirken öncelikle **Inbox Table** adını verdiğimiz bir tabloya işlenmeli ardından process edilmelidirler. Inbox table’da eğer ki aynı ‘IdempotentToken’ değerine sahip bir kayıt varsa işlem gerçekleştirilmemeli, eğer yoksa process edilmelidir. Burada kullanılan yöntem özünde **Inbox Pattern**‘ı barındırmaktadır.

## Inbox Pattern

Outbox Pattern’a benzer bir yapıya sahiptir. Outbox Pattern’da işlemler gerçekleştirildikten sonra yayınlanacak mesajlar Outbox table’a kaydedilir ve ardından bir publisher aracılığıyla yayınlanırken, Inbox Pattern’da işlenecek mesajlar önce Inbox table’a eklenir ve daha sonra işlenirler. Örneğin, bir siparişi veritabanına kaydetmek ve ardından stok bilgisini güncellemek için Outbox Pattern kullanılırken, stok bilgilerini güncellemekten sorumlu olan consumer, ilgili mesajları Inbox table’a işleyip daha sonra işleme alır. Ancak burada, işlenen mesajların Inbox table’da tamamlandığına dair güncelleme yapılması gereklidir.

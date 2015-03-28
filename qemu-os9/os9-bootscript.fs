here >r
dev /
" model" active-package get-package-property abort" can't find MODEL"
decode-string 2swap 2drop " iMac,1" $= ?dup 0= if
 " compatible" active-package get-package-property abort" can't find COMPATIBLE"
 false >r
 begin
  dup while
  decode-string here over 2swap bounds ?do
   i c@ dup [char] A [char] Z between if h# 20 xor then c,
   loop
  2dup " macrisc" $= r> or >r
  2drop
  repeat
 2drop r>
  then
r> here - allot
0= abort" this image is not for this platform"
decimal
1 load-base load-size 14 - adler32    load-base load-size + 12 - 12 ['] eval catch if
 2drop ." , bad checksum value" -1
  then
 <> if
 ." , checksum error"
 abort
  then
hex
dev /openprom
0 0 " supports-bootinfo" property  device-end
" /chosen" find-package 0= abort" can't find '/chosen'" constant /chosen
" memory" /chosen get-package-property abort" memory??" decode-int constant xmem 2drop
" mmu" /chosen get-package-property abort" mmu??" decode-int constant xmmu 2drop
" AAPL,debug" " /" find-package 0= abort" can't find '/'" get-package-property if
   false
  else
 2drop true
  then
 constant debug?
debug? if cr ." checking for RELEASE-LOAD-AREA" then
" release-load-area" $find 0= if 2drop false then  constant 'release-load-area
debug? if 'release-load-area if ." , found it" else ." , not found" then then
: do-translate " translate" xmmu $call-method ;
: do-map  " map" xmmu $call-method ;
: do-unmap " unmap" xmmu $call-method ;
: claim-mem  " claim" xmem $call-method ;
: release-mem " release" xmem $call-method ;
: claim-virt " claim" xmmu $call-method ;
: release-virt " release" xmmu $call-method ;
1000 constant pagesz
pagesz 1- constant pagesz-1
-1000 constant pagemask
h# 005000 constant elf-offset
h# 017008 constant elf-size
elf-size pagesz-1 + pagemask and constant elf-pages
h# 01C008 constant parcels-offset
h# 261958 constant parcels-size
parcels-size pagesz-1 + pagemask and constant parcels-pages
h# 27D960 constant info-size
info-size pagesz-1 + pagemask and constant info-pages
0 value load-base-claim
0 value info-base
'release-load-area if
    load-base to info-base
  else
    load-base info-pages 0 ['] claim-mem catch if 3drop 0 then to load-base-claim
    info-pages 1000 claim-virt to info-base
    load-base info-base info-pages 10 do-map   then
\ allocate room for both images
parcels-pages 400000 claim-mem constant rom-phys parcels-pages 1000 claim-virt constant rom-virt rom-phys rom-virt parcels-pages 10 do-map  
elf-pages 1000 claim-mem constant elf-phys   elf-pages 1000 claim-virt constant elf-virt
elf-phys elf-virt elf-pages 10 do-map    info-base elf-offset + elf-virt elf-size move  debug? if cr ." elf-phys,elf-virt,elf-pages: " elf-phys u. ." , " elf-virt u. ." , " elf-pages u. then
\ copy the compressed image
debug? if cr ." copying compressed ROM image" then
rom-virt parcels-pages 0 fill
info-base parcels-offset + rom-virt parcels-size move
'release-load-area 0= if
    info-base info-pages do-unmap      load-base-claim ?dup if info-pages release-mem then
  then
debug? if cr ." MacOS-ROM phys,virt,size: " rom-phys u. ." , " rom-virt u. ." , " parcels-size u. then
\ create the actual property
debug? if cr ." finding/creating '/rom/macos' package" then
device-end 0 to my-self
" /rom" find-device
" macos" ['] find-device catch if 2drop new-device " macos" device-name finish-device then
" /rom/macos" find-device
debug? if cr ." creating 'AAPL,toolbox-parcels' property" then
rom-virt encode-int parcels-size encode-int encode+ " AAPL,toolbox-parcels" property
device-end
debug? if cr ." copying MacOS.elf to load-base" then
'release-load-area if
    load-base elf-pages + 'release-load-area execute
  else
    load-base elf-pages 0 claim-mem
    load-base dup elf-pages 0 do-map    then
elf-virt load-base elf-size move
elf-virt elf-pages do-unmap      elf-virt elf-pages release-virt
elf-phys elf-pages release-mem
debug? if cr ." init-program" then
init-program
debug? if cr ." .registers" .registers then
debug? if cr ." go" cr then
go
cr ." end of BOOT-SCRIPT"

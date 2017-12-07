using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchIris
{
    public class Matching
    {
        private IrisCode irisCode;
        private IrisCode matchIrisCode;

        public Matching(IrisCode irisCode, IrisCode matchIrisCode)
        {
            this.irisCode = irisCode;
            this.matchIrisCode = matchIrisCode;
        }

        public Matching(IrisCode irisCode)
        {
            this.irisCode = irisCode;
        }

        public Matching()
        {

        }

        public float PerformMatching()
        {
            int i;
            IrisCode tmp_irisCode = new IrisCode();
            tmp_irisCode.size = matchIrisCode.size;
            tmp_irisCode.bit = matchIrisCode.bit;
            tmp_irisCode.mask = matchIrisCode.mask;

            float HD = 0;
            float HDl = 0;
            float HDr = 0;
            float max_HD = 1;

            for (i = 0; i < 10; i++)
            {
                tmp_irisCode = shift_left(i);
                HDl = find_HD_match(irisCode, tmp_irisCode);
                tmp_irisCode = shift_right(i);
                HDr = find_HD_match(irisCode, tmp_irisCode);
                                
                if (HDr < HDl)
                {
                    HD = HDr;
                }
                else
                {
                    HD = HDl;
                }
                if (HD < max_HD)
                {
                    max_HD = HD;
                }
            }

            return max_HD;
        }

        public float PerformByteMatch(IrisCode probe, IrisCode candidate, int max_shift)
        {
            float best_value = 1.0f;

            for (int i = 0; i < max_shift; i++)
            {
                uint[] bit_geser_candidate_left = CyclicShiftLeft(i, candidate.newBit);
                uint[] bit_geser_candidate_right = CyclicShiftRight(i, candidate.newBit);
                uint[] mask_geser_candidate_left = CyclicShiftLeft(i, candidate.newMask);
                uint[] mask_geser_candidate_right = CyclicShiftRight(i, candidate.newMask);

                float result_kiri = BitwiseVerify(probe.newBit, bit_geser_candidate_left, probe.newMask, mask_geser_candidate_left);
                float result_kanan = BitwiseVerify(probe.newBit, bit_geser_candidate_right, probe.newMask, mask_geser_candidate_right);

                if (result_kiri < best_value)
                {
                    best_value = result_kiri;
                }
                if (result_kanan < best_value)
                {
                    best_value = result_kanan;
                }
            }

            return best_value;
        }

        public float BitwiseVerify(uint[] bitProbe, uint[] bitCandidate, uint[] maskProbe, uint[] maskCandidate)
        {
            int panjang = bitCandidate.Length;
            uint total_length = 0;
            uint total_penyebut = 0;

            uint[] result = new uint[panjang];
            uint[] penyebut = new uint[panjang];
            uint[] operand = new uint[panjang];
            float fullres = 0.0f;

            uint[] full_HD = new uint[panjang];

            for (int i = 0; i < panjang; i++)
            {
                operand[i] = ~(maskProbe[i] | maskCandidate[i]);
                full_HD[i] = (bitProbe[i] ^ bitCandidate[i]) & operand[i];
                result[i] = CountOnes.pop(full_HD[i]);
                penyebut[i] = CountOnes.pop(operand[i]);
                total_length += result[i];
                total_penyebut += penyebut[i];
            }

            fullres = (float)total_length / (float)(total_penyebut);

            return fullres;
        }

        public IrisCode shift_left(int degree)
        {
            IrisCode irisCodeResult = new IrisCode();
            int length = matchIrisCode.size;

            irisCodeResult.size = length;

            int j, c, d;

            byte[] temp = new byte[length];
            byte[] temp_mask = new byte[length];

            for (j = 0; j < length; j++)
            {
                c = j + degree;
                if (c >= length)
                    d = c - length;
                else
                    d = c;
                temp[j] = matchIrisCode.bit[d];
                temp_mask[j] = matchIrisCode.mask[d];
            }

            irisCodeResult.bit = temp;
            irisCodeResult.mask = temp_mask;

            return irisCodeResult;
        }

        public IrisCode shift_right(int degree)
        {
            IrisCode irisCodeResult = new IrisCode();
            int length = matchIrisCode.size;

            irisCodeResult.size = length;

            int j, c, d;

            byte[] temp = new byte[length];
            byte[] temp_mask = new byte[length];

            for (j = length - 1; j >= 0; j--)
            {
                c = j - degree;
                if (c < 0)
                    d = c + length;
                else
                    d = c;
                temp[j] = matchIrisCode.bit[d];
                temp_mask[j] = matchIrisCode.mask[d];
            }

            irisCodeResult.bit = temp;
            irisCodeResult.mask = temp_mask;

            return irisCodeResult;
        }

        public float find_HD_match(IrisCode irisCode, IrisCode matchIrisCode)
        {
            int j;

            int length = matchIrisCode.size;
            if (irisCode.size < length)
            {
                length = irisCode.size;
            }
            int diff_c = 0;
            int usable_bit = 0;
            float HD = 0;
            for (j = 0; j < length; j++)
            {
                if (irisCode.mask[j] == 0 && matchIrisCode.mask[j] == 0)
                {
                    if (matchIrisCode.bit[j] != irisCode.bit[j])
                        diff_c++;
                    usable_bit++;
                }
            }
            HD = (float)diff_c / usable_bit;

            return HD;
        }

        public static uint[] CyclicShiftLeft(int border, uint[] masuk)
        {
            uint[] keluaran = new uint[masuk.Length];

            uint bit_before_left, bit_before_right;
            uint bit_after_left, bit_after_right;
            uint bit_merge;

            int shift_dist = border;
            int counter_keluaran = 0;
            int geser;
            
            //int pembagi = (int)border / 32;
            //int sisa = border - (32 * pembagi);
            int pembagi = (int)border >> 5; //division by 32
            int sisa = border - (pembagi << 5); //multiplication by 32

            shift_dist = sisa;
            geser = 32 - shift_dist;

            for (int i = pembagi; i < masuk.Length - 1; i++)
            {
                bit_before_left = masuk[i];
                bit_before_right = masuk[i + 1];

                bit_after_left = bit_before_left << shift_dist;
                bit_after_right = (sisa == 0) ? 0 : bit_before_right >> geser;

                bit_merge = bit_after_left | bit_after_right;
                keluaran[counter_keluaran] = bit_merge;

                counter_keluaran++;
            }

            #region Connector
            bit_before_left = masuk[masuk.Length - 1];
            bit_before_right = masuk[0];

            bit_after_left = bit_before_left << shift_dist;
            bit_after_right = (sisa == 0) ? 0 : bit_before_right >> geser;

            bit_merge = bit_after_left | bit_after_right;

            keluaran[counter_keluaran] = bit_merge;

            counter_keluaran++;
            #endregion

            for (int j = 0; j < pembagi; j++)
            {
                bit_before_left = masuk[j];
                bit_before_right = masuk[j + 1];

                bit_after_left = bit_before_left << shift_dist;
                bit_after_right = (sisa == 0) ? 0 : bit_before_right >> geser;

                bit_merge = bit_after_left | bit_after_right;

                keluaran[counter_keluaran] = bit_merge;

                counter_keluaran++;
            }

            return keluaran;
        }

        public static uint[] CyclicShiftRight(int border, uint[] masuk)
        {
            uint[] keluaran = new uint[masuk.Length];

            uint bit_before_left, bit_before_right;
            uint bit_after_left, bit_after_right;
            uint bit_merge;

            int shift_dist = border;
            int counter_keluaran = 0;
            int geser;

            //int pembagi = (int)border / 32;
            //int sisa = border - (32 * pembagi);
            int pembagi = (int)border >> 5; //division by 32
            int sisa = border - (pembagi << 5); //multiplication by 32

            shift_dist = sisa;
            geser = 32 - shift_dist;

            for (int j = pembagi; j > 0; j--)
            {
                bit_before_left = masuk[masuk.Length - j - 1];
                bit_before_right = masuk[masuk.Length - j];

                bit_after_left = (sisa == 0) ? 0 : bit_before_left << geser;
                bit_after_right = bit_before_right >> shift_dist;

                bit_merge = bit_after_left | bit_after_right;
                keluaran[counter_keluaran] = bit_merge;

                counter_keluaran++;
            }

            #region Connector
            bit_before_left = masuk[masuk.Length - 1];
            bit_before_right = masuk[0];

            bit_after_left = (sisa == 0) ? 0 : bit_before_left << geser;
            bit_after_right = bit_before_right >> shift_dist;

            bit_merge = bit_after_left | bit_after_right;

            keluaran[counter_keluaran] = bit_merge;
            counter_keluaran++;
            #endregion

            for (int i = 1; i < masuk.Length - pembagi; i++)
            {
                bit_before_left = masuk[i - 1];
                bit_before_right = masuk[i];

                bit_after_left = (sisa == 0) ? 0 : bit_before_left << geser;
                bit_after_right = bit_before_right >> shift_dist;

                bit_merge = bit_after_left | bit_after_right;
                keluaran[counter_keluaran] = bit_merge;

                counter_keluaran++;
            }

            return keluaran;

        }

    }
}

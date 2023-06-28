import requests
import json


api_url = "http://192.168.1.73:8192/star/inference"

from datetime import datetime
for _ in range(10):
    t1 = datetime.now()
    data = {
        "inputs": """
        void OnEnable()
        {
            // For some reason, when entering playmode, CreateGUI is not called again so we need to rebuild the UI because all references are lost.
            // Apparently it was not the case a while ago.
            if (m_Root == null)
                RebuildUI();

            for (int i = 0; i < (int)RenderGraphResourceType.Count; ++i)
                m_ResourceElementsInfo[i] = new DynamicArray<ResourceElementInfo>();

            var registeredGraph = RenderGraph.GetRegisteredRenderGraphs();
            foreach (var graph in registeredGraph)
                m_RegisteredGraphs.Add(graph, new HashSet<string>());

            RenderGraph.requireDebugData = true;
            RenderGraph.registeredRenderGraphsChanged += OnRegisteredRenderGraphsChanged;
            RenderGraph.renderGraphChanged
        """,
        "parameters": {"max_new_tokens": 60}
    }

    # 发送 POST 请求
    response = requests.post(api_url, json=data)
    t2 = datetime.now()

    t3 = datetime.now()
    data2 = {
        "inputs" : """
        <fim_prefix> import copy
        import math

        import numpy as np
        import torch
        from configuration_bertabs import BertAbsConfig
        from torch import nn
        from torch.nn.init import xavier_uniform_

        from transformers import BertConfig, BertModel, PreTrainedModel


        MAX_SIZE = 5000

        BERTABS_FINETUNED_MODEL_ARCHIVE_LIST = [
            "remi/bertabs-finetuned-cnndm-extractive-abstractive-summarization",
        ]


        class BertAbsPreTrainedModel(PreTrainedModel):
            config_class = BertAbsConfig
            load_tf_weights = False
            base_model_prefix = "bert"


        class BertAbs(BertAbsPreTrainedModel):
            def __init__(self, args, checkpoint=None, bert_extractive_checkpoint=None):
                super().__init__(args)
                self.args = args
                self.bert = Bert()

                # If pre-trained weights are passed for Bert, load these.
                load_bert_pretrained_extractive = True if bert_extractive_checkpoint else False
                if load_bert_pretrained_extractive:
                    self.bert.model.load_state_dict(
                        {n[11:]: p for n, p in bert_extractive_checkpoint.items() if n.startswith("bert.model")},
                        strict=True,
                    )

                self.vocab_size = self.bert.model.config.vocab_size

                if args.max_pos > 512:
                    my_pos_embeddings = nn.Embedding(args.max_pos, self.bert.model.config.hidden_size)
                    my_pos_embeddings.weight.data[:512] = self.bert.model.embeddings.position_embeddings.weight.data
                    my_pos_embeddings.weight.data[512:] = self.bert.model.embeddings.position_embeddings.weight.data[-1][
                        None, :
                    ].repeat(args.max_pos - 512, 1)
                    self.bert.model.embeddings.position_embeddings = my_pos_embeddings
                tgt_embeddings = nn.Embedding(self.vocab_size, self.bert.model.config.hidden_size, padding_idx=0)

                tgt_embeddings.weight = copy.deepcopy(self.bert.model.embeddings.word_embeddings.weight)

                self.decoder = TransformerDecoder(
                    self.args.dec_layers,
                    self.args.dec_hidden_size,
                    heads=self.args.dec_heads,
                    d_ff=self.args.dec_ff_size,
                    dropout=self.args.dec_dropout,
                    embeddings=tgt_embeddings,
                    vocab_size=self.vocab_size,
                )

                gen_func = nn.LogSoftmax(dim=-1)
                self.generator = nn.Sequential(nn.Linear(args.dec_hidden_size, args.vocab_size), gen_func)
                self.generator[0].weight = self.decoder.embeddings.weight

                load_from_checkpoints = False if checkpoint is None else True
                if load_from_checkpoints:
                    self.load_state_dict(checkpoint)

            def init_weights(self):
                for module in self.decoder.modules():
                    <fim_suffix>
                for p in self.generator.parameters():
                    if p.dim() > 1:
                        xavier_uniform_(p)
                    else:
                        p.data.zero_()

            def forward(
                self,
                encoder_input_ids,
                decoder_input_ids,
                token_type_ids,
                encoder_attention_mask,
                decoder_attention_mask,
            ):
                encoder_output = self.bert(
                    input_ids=encoder_input_ids,
                    token_type_ids=token_type_ids,
                    attention_mask=encoder_attention_mask,
                )
                encoder_hidden_states = encoder_output[0]
                dec_state = self.decoder.init_decoder_state(encoder_input_ids, encoder_hidden_states)
                decoder_outputs, _ = self.decoder(decoder_input_ids[:, :-1], encoder_hidden_states, dec_state)
                return decoder_outputs


        class Bert(nn.Module):

            def __init__(self):
                super().__init__()
                config = BertConfig.from_pretrained("bert-base-uncased")
                self.model = BertModel(config)

            def forward(self, input_ids, attention_mask=None, token_type_ids=None, **kwargs):
                self.eval()
                with torch.no_grad():
                    encoder_outputs, _ = self.model(
                        input_ids, token_type_ids=token_type_ids, attention_mask=attention_mask, **kwargs
                    )
                return encoder_outputs <fim_middle>
        """,
        "parameters": {"max_new_tokens": 60}
    }
    response2 = requests.post(api_url, json=data2)
    t4 = datetime.now()

    t5 = datetime.now()
    data3 = {
        "inputs" : """
        <fim_prefix> #endregion

        using System.Collections.Generic;

        namespace OpenRA.Mods.Common.Terrain
        {
            public interface ITemplatedTerrainInfo : ITerrainInfo
            {
                string[] EditorTemplateOrder { get; }
                IReadOnlyDictionary<ushort, TerrainTemplateInfo> Templates { get; }
            }

            public interface ITerrainInfoNotifyMapCreated : ITerrainInfo
            {
                void MapCreated(Map map);
            }

            public class TerrainTemplateInfo
            {
                public readonly ushort Id;
                public readonly int2 Size;
                public readonly bool PickAny;
                public readonly string[] Categories;

                readonly TerrainTileInfo[] tileInfo;

                public TerrainTemplateInfo(ITerrainInfo terrainInfo, MiniYaml my)
                {
                    FieldLoader.Load(this, my);

                    var nodes = my.ToDictionary()["Tiles"].Nodes;

                    if (!PickAny)
                    {
                        tileInfo = new TerrainTileInfo[Size.X * Size.Y];
                        foreach (var node in nodes)
                        {
                            <fim_middle>
                        }
                    }
                    else
                    {
                        tileInfo = new TerrainTileInfo[nodes.Count];

                        var i = 0;
                        foreach (var node in nodes)
                        {
                            if (!int.TryParse(node.Key, out var key))
                                throw new YamlException($"Tileset `{terrainInfo.Id}` template `{Id}` defines a frame `{node.Key}` that is not a valid integer.");

                            if (key != i++)
                                throw new YamlException($"Tileset `{terrainInfo.Id}` template `{Id}` is missing a definition for frame {i - 1}.");

                            tileInfo[key] = LoadTileInfo(terrainInfo, node.Value);
                        }
                    }
                }

                protected virtual TerrainTileInfo LoadTileInfo(ITerrainInfo terrainInfo, MiniYaml my)
                {
                    var tile = new TerrainTileInfo();
                    FieldLoader.Load(tile, my);

                    // Terrain type must be converted from a string to an index
                    tile.GetType().GetField(nameof(tile.TerrainType)).SetValue(tile, terrainInfo.GetTerrainIndex(my.Value));

                    // Fall back to the terrain-type color if necessary
                    var overrideColor = terrainInfo.TerrainTypes[tile.TerrainType].Color;
                    if (tile.MinColor == default)
                        tile.GetType().GetField(nameof(tile.MinColor)).SetValue(tile, overrideColor);

                    if (tile.MaxColor == default)
                        tile.GetType().GetField(nameof(tile.MaxColor)).SetValue(tile, overrideColor);

                    return tile;
                }

                public TerrainTileInfo this[int index] => tileInfo[index];

                public bool Contains(int index)
                {
                    return index >= 0 && index < tileInfo.Length;
                }

                public int TilesCount => tileInfo.Length;
            }
        } <fim_middle>
        """,
        "parameters": {"max_new_tokens": 60}
    }
    response3 = requests.post(api_url, json=data3)
    t6 = datetime.now()

    t7 = datetime.now()
    data4 = {
        "inputs" : """
        <fim_prefix> package com.google.thirdparty.publicsuffix;

        import static com.google.common.collect.Queues.newArrayDeque;

        import com.google.common.annotations.GwtCompatible;
        import com.google.common.annotations.VisibleForTesting;
        import com.google.common.base.Joiner;
        import com.google.common.collect.ImmutableMap;
        import java.util.Deque;

        /** Parser for a map of reversed domain names stored as a serialized radix tree. */
        @GwtCompatible
        final class TrieParser {

        private static final Joiner DIRECT_JOINER = Joiner.on("");

        /**
        * Parses a serialized trie representation of a map of reversed public suffixes into an immutable
        * map of public suffixes. The encoded trie string may be broken into multiple chunks to avoid the
        * 64k limit on string literal size. In-memory strings can be much larger (2G).
        */
        static ImmutableMap<String, PublicSuffixType> parseTrie(CharSequence... encodedChunks) {
            String encoded = DIRECT_JOINER.join(encodedChunks);
            return parseFullString(encoded);
        }

        @VisibleForTesting
        static ImmutableMap<String, PublicSuffixType> parseFullString(String encoded) {
            ImmutableMap.Builder<String, PublicSuffixType> builder = ImmutableMap.builder();
            int encodedLen = encoded.length();
            int idx = 0;

            while (idx < encodedLen) {
            idx += doParseTrieToBuilder(newArrayDeque(), encoded, idx, builder);
            }

            return builder.buildOrThrow();
        }

        /**
        * Parses a trie node and returns the number of characters consumed.
        *
        * @param stack The prefixes that precede the characters represented by this node. Each entry of
        *     the stack is in reverse order.
        * @param encoded The serialized trie.
        * @param start An index in the encoded serialized trie to begin reading characters from.
        * @param builder A map builder to which all entries will be added.
        * @return The number of characters consumed from {@code encoded}.
        */
        private static int doParseTrieToBuilder(
            Deque<CharSequence> stack,
            CharSequence encoded,
            int start,
            ImmutableMap.Builder<String, PublicSuffixType> builder) {

            int encodedLen = encoded.length();
            int idx = start;
            char c = '\0';

            // Read all the characters for this node.
            for (; idx < encodedLen; idx++) {
                <fim_suffix>
            }

            stack.push(reverse(encoded.subSequence(start, idx)));

            if (c == '!' || c == '?' || c == ':' || c == ',') {
            // '!' represents an interior node that represents a REGISTRY entry in the map.
            // '?' represents a leaf node, which represents a REGISTRY entry in map.
            // ':' represents an interior node that represents a private entry in the map
            // ',' represents a leaf node, which represents a private entry in the map.
            String domain = DIRECT_JOINER.join(stack);

            if (domain.length() > 0) {
                builder.put(domain, PublicSuffixType.fromCode(c));
            }
            }

            idx++;

            if (c != '?' && c != ',') {
            while (idx < encodedLen) {
                // Read all the children
                idx += doParseTrieToBuilder(stack, encoded, idx, builder);

                if (encoded.charAt(idx) == '?' || encoded.charAt(idx) == ',') {
                // An extra '?' or ',' after a child node indicates the end of all children of this node.
                idx++;
                break;
                }
            }
            }

            stack.pop();
            return idx - start;
        }

        private static CharSequence reverse(CharSequence s) {
            return new StringBuilder(s).reverse();
        }
        } <fim_middle>
        """,
        "parameters": {"max_new_tokens": 60}
    }
    response4 = requests.post(api_url, json=data4)
    t8 = datetime.now()
    
    t9 = datetime.now()
    data5 = {
        "inputs" : """
            <fim_prefix> #ifndef OPENPOSE_THREAD_W_ID_GENERATOR_HPP
            #define OPENPOSE_THREAD_W_ID_GENERATOR_HPP

            #include <queue> // std::priority_queue
            #include <openpose/core/common.hpp>
            #include <openpose/thread/worker.hpp>
            #include <openpose/utilities/pointerContainer.hpp>

            namespace op
            {
                template<typename TDatums>
                class WIdGenerator : public Worker<TDatums>
                {
                public:
                    explicit WIdGenerator();

                    virtual ~WIdGenerator();

                    void initializationOnThread();

                    void work(TDatums& tDatums);

                private:
                    unsigned long long mGlobalCounter;

                    DELETE_COPY(WIdGenerator);
                };
            }





            // Implementation
            #include <openpose/utilities/pointerContainer.hpp>
            namespace op
            {
                template<typename TDatums>
                WIdGenerator<TDatums>::WIdGenerator() :
                    mGlobalCounter{0ull}
                {
                }

                template<typename TDatums>
                WIdGenerator<TDatums>::~WIdGenerator()
                {
                }

                template<typename TDatums>
                void WIdGenerator<TDatums>::initializationOnThread()
                {
                }

                template<typename TDatums>
                void WIdGenerator<TDatums>::work(TDatums& tDatums)
                {
                    try
                    {
                        if (checkNoNullNorEmpty(tDatums))
                        {
                            // Debugging log
                            opLogIfDebug("", Priority::Low, __LINE__, __FUNCTION__, __FILE__);
                            // Profiling speed
                            const auto profilerKey = Profiler::timerInit(__LINE__, __FUNCTION__, __FILE__);
                            // Add ID
                            for (auto& tDatumPtr : *tDatums)
                                // To avoid overwriting ID if e.g., custom input has already filled it
                                <fim_suffix>
                            // Increase ID
                            const auto& tDatumPtr = (*tDatums)[0];
                            if (tDatumPtr->subId == tDatumPtr->subIdMax)
                                mGlobalCounter++;
                            // Profiling speed
                            Profiler::timerEnd(profilerKey);
                            Profiler::printAveragedTimeMsOnIterationX(profilerKey, __LINE__, __FUNCTION__, __FILE__);
                            // Debugging log
                            opLogIfDebug("", Priority::Low, __LINE__, __FUNCTION__, __FILE__);
                        }
                    }
                    catch (const std::exception& e)
                    {
                        this->stop();
                        tDatums = nullptr;
                        error(e.what(), __LINE__, __FUNCTION__, __FILE__);
                    }
                }

                COMPILE_TEMPLATE_DATUM(WIdGenerator);
            }

            #endif // OPENPOSE_THREAD_W_ID_GENERATOR_HPP <fim_middle>
        """,
        "parameters": {"max_new_tokens": 60}
    }
    response5 = requests.post(api_url, json=data5)
    t10 = datetime.now()
    
    t11 = datetime.now()
    data6 = {
        "inputs" : """
            <fim_prefix> int
            myfread(char *buf, int elsize /*ignored*/, int max, FILE *fp)
            {
                int	c;
                int	n = 0;

                while ((n < max) && ((c = getchar()) != EOF))
                {
                    *(buf++) = c;
                    n++;
                    if (c == '\n' || c == '\r')
                        break;
                }
                return n;
            }


            int
            main(int argc, char *argv[])
            {
                int	append = 0;
                size_t	numfiles;
                int	maxfiles;
                FILE	**filepointers;
                int	i;
                char	buf[BUFSIZ];
                int	n;
                int	optind = 1;

                for (i = 1; i < argc; i++)
                {
                    if (argv[i][0] != '-')
                        break;
                    if (!strcmp(argv[i], "-a"))
                        append++;
                    else
                        usage();
                    optind++;
                }

                numfiles = argc - optind;

                if (numfiles == 0)
                {
                    fprintf(stderr, "doesn't make much sense using tee without any file name arguments...\n");
                    usage();
                    exit(2);
                }

                maxfiles = sysconf(_SC_OPEN_MAX);	/* or fill in 10 or so */
                if (maxfiles < 0)
                    maxfiles = 10;
                if (numfiles + 3 > maxfiles)	/* +3 accounts for stdin, out, err */
                {
                    fprintf(stderr, "Sorry, there is a limit of max %d files.\n", maxfiles - 3);
                    exit(1);
                }
                filepointers = calloc(numfiles, sizeof(FILE *));
                if (filepointers == NULL)
                {
                    fprintf(stderr, "Error allocating memory for %ld files\n",
                                                                        (long)numfiles);
                    exit(1);
                }
                for (i = 0; i < numfiles; i++)
                {
                    <fim_suffix>
                }
            #ifdef _WIN32
                setmode(fileno(stdin),  O_BINARY);
                fflush(stdout);	/* needed for _fsetmode(stdout) */
                setmode(fileno(stdout),  O_BINARY);
            #endif

                while ((n = myfread(buf, sizeof(char), sizeof(buf), stdin)) > 0)
                {
                    fwrite(buf, sizeof(char), n, stdout);
                    fflush(stdout);
                    for (i = 0; i < numfiles; i++)
                    {
                        if (filepointers[i] &&
                            fwrite(buf, sizeof(char), n, filepointers[i]) != n)
                        {
                            fprintf(stderr, "Error writing to file \"%s\"\n", argv[i+optind]);
                            fclose(filepointers[i]);
                            filepointers[i] = NULL;
                        }
                    }
                }
                for (i = 0; i < numfiles; i++)
                {
                    if (filepointers[i])
                        fclose(filepointers[i]);
                }

                exit(0);
            } <fim_middle>
        """,
        "parameters": {"max_new_tokens": 60}
    }
    response6 = requests.post(api_url, json=data6)
    t12 = datetime.now()

    
    print(response2.content)
    print(f"time elapsed = {t2-t1}")
    print(f"Python time elapsed = {t4-t3}")
    print(f"C# time elapsed = {t6-t5}")
    print(f"Java time elapsed = {t8-t7}")
    print(f"C++ time elapsed = {t10-t9}")
    print(f"C completion time elapsed = {t12-t11}")
    
